using AutoMapper;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using ServiceStack.Messaging;
using ServiceStack.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using Wasenshi.AuthPolicy.Attributes;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Models.Enums;
using Wasenshi.HemoDialysisPro.Models.Settings;
using Wasenshi.HemoDialysisPro.Services.Interfaces;
using Wasenshi.HemoDialysisPro.Share;
using Wasenshi.HemoDialysisPro.Models.ConfigExtesions;
using Wasenshi.HemoDialysisPro.ViewModels;
using Wasenshi.HemoDialysisPro.Web.Api.BackgroundTasks;
using Wasenshi.HemoDialysisPro.Web.Api.Controllers.Utils;
using Wasenshi.HemoDialysisPro.Utils;
using System.Linq.Expressions;
using Wasenshi.HemoDialysisPro.Web.Api.Utils;
using Wasenshi.AuthPolicy;

namespace Wasenshi.HemoDialysisPro.Web.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ScheduleController : ControllerBase
    {
        private readonly IScheduleService scheduleService;
        private readonly IShiftService shiftService;
        private readonly IMapper mapper;
        private readonly IMasterDataService masterData;
        private readonly IAuthService auth;
        private readonly IPatientService patientService;
        private readonly IWritableOptions<UnitSettings> unitSetting;
        private readonly IWritableOptions<GlobalSetting> setting;
        private readonly IMessageQueueClient message;
        private readonly IBackgroundJobClient bgJob;
        private readonly IRedisClient redis;
        private readonly IConfiguration config;

        public ScheduleController(
            IScheduleService scheduleService,
            IShiftService shiftService,
            IMapper mapper,
            IMasterDataService masterData,
            IAuthService auth,
            IPatientService patientService,
            IWritableOptions<UnitSettings> unitSetting,
            IWritableOptions<GlobalSetting> setting,
            IMessageQueueClient messageQueueClient,
            IBackgroundJobClient bgJob,
            IRedisClient redis,
            IConfiguration config)
        {
            this.scheduleService = scheduleService;
            this.shiftService = shiftService;
            this.mapper = mapper;
            this.masterData = masterData;
            this.auth = auth;
            this.patientService = patientService;
            this.unitSetting = unitSetting;
            this.setting = setting;
            this.message = messageQueueClient;
            this.bgJob = bgJob;
            this.redis = redis;
            this.config = config;
        }

        [HttpGet("today-patient")]
        public IActionResult GetTodayPatient(int? unitId = null)
        {
            if (unitId.HasValue && !auth.VerifyUnit(User, new[] { unitId.Value }))
            {
                return Forbid();
            }

            var unitList = unitId.HasValue ? new[] { unitId.Value } : User.GetUnitList().ToArray();
            // doctor patient access
            Expression<Func<Schedule, bool>> whereCondition = null;
            Expression<Func<SectionSlotPatient, bool>> whereConditionSlot = null;
            if (this.IsDoctorAndSeeOwnPatientOnly(setting.Value))
            {
                var doctorId = User.GetUserIdAsGuid();
                whereCondition = x => x.Patient.DoctorId == doctorId;
                whereConditionSlot = x => x.Patient.DoctorId == doctorId;
            }

            var schedules = scheduleService.GetActiveSchedulesForToday(unitList, whereCondition).ToList();
            var slots = scheduleService.GetActiveSlotForToday(unitList, whereConditionSlot).ToList();

            var patients = schedules.Select(x =>
            {
                var map = mapper.Map<PatientViewModel>(x.Patient);
                map.Schedule = x.Date;
                return map;
            }).Concat(slots.Select(x => mapper.Map<PatientViewModel>(x.Patient))).GroupBy(x => x.Id).Select(x => x.First())
            .ToList();

            patients.SetSessionForPatients(redis);

            return Ok(patients);
        }

        [HttpGet("section/{sectionId}")]
        public IActionResult GetSection(int sectionId)
        {
            ScheduleSection section = scheduleService.GetSection(sectionId);

            if (section == null)
            {
                return Ok(null);
            }

            return Ok(mapper.Map<ScheduleSectionViewModel>(section));
        }

        [HttpGet("{unitId}/sections")]
        public IActionResult GetSections(int unitId, bool pendingChange = false)
        {
            IEnumerable<ScheduleSection> sections = scheduleService.GetSections(unitId);
            var result = new GetSectionsViewModel
            {
                Sections = mapper.Map<IEnumerable<ScheduleSectionViewModel>>(sections)
            };
            if (pendingChange)
            {
                IEnumerable<TempSection> tempSections = scheduleService.GetTempSections(unitId);
                var pending = new List<ScheduleSectionViewModel>();
                foreach (TempSection item in tempSections)
                {
                    if (!item.Delete)
                    {
                        pending.Add(mapper.Map<ScheduleSectionViewModel>(item));
                    }
                }
                result.Pendings = pending;
            }

            return Ok(result);
        }

        [HttpGet("{unitId}/sections/pending")]
        public IActionResult GetPendingSections(int unitId)
        {
            IEnumerable<TempSection> tempSections = scheduleService.GetTempSections(unitId);
            var pending = new List<ScheduleSectionViewModel>();
            foreach (TempSection item in tempSections)
            {
                if (!item.Delete)
                {
                    pending.Add(mapper.Map<ScheduleSectionViewModel>(item));
                }
            }

            return Ok(pending);
        }

        [Authorize(Roles = Roles.HeadNurseOnly)]
        [HttpPost("{unitId}/sections/update")]
        public IActionResult UpdateSections(int unitId, [FromBody] UpdateUnitSectionViewModel updateModel)
        {
            if (!auth.VerifyUnit(User, new[] { unitId }))
            {
                return Forbid();
            }
            this.ValidateUnitHeadOrInCharged(unitId, masterData, shiftService, redis);

            bool isTest = config.GetValue<bool>("TESTING");

            DateTimeOffset? targetEffectiveDate = updateModel.TargetEffectiveDate;
            bool scheduled = targetEffectiveDate.HasValue;
            if (scheduled)
            {
                var tmpSections = mapper.Map<IEnumerable<TempSection>>(updateModel.SectionList);
                var deletes = mapper.Map<IEnumerable<TempSection>>(updateModel.DeleteList);
                if (deletes?.Any() ?? false)
                {
                    tmpSections = tmpSections.Concat(deletes);
                }
                var result = scheduleService.CreateOrUpdateTempSections(unitId, tmpSections);
                if (result)
                {
                    if (!isTest)
                    {
                        // queue schedule update
                        bgJob.QueuePendingSectionUpdate(new PendingSectionUpdate { UnitId = unitId, TargetDate = targetEffectiveDate.Value }, redis);
                    }

                    return Ok(mapper.Map<IEnumerable<ScheduleSectionViewModel>>(tmpSections.Where(x => !x.Delete)));
                }

                return BadRequest();
            }

            var sections = mapper.Map<IEnumerable<ScheduleSection>>(updateModel.SectionList);

            int count = scheduleService.CreateOrUpdateSections(unitId,
                sections, mapper.Map<IEnumerable<ScheduleSection>>(updateModel.DeleteList));

            if (count == 0)
            {
                return NotFound();
            }


            // update meta data
            message.Publish(new SectionUpdated { UnitId = unitId });

            return Ok(mapper.Map<IEnumerable<ScheduleSectionViewModel>>(sections));
        }

        [Authorize(Roles = Roles.HeadNurseOnly)]
        [HttpPut("{unitId}/sections/clear-pending")]
        public IActionResult ClearSectionsPending(int unitId)
        {
            if (!auth.VerifyUnit(User, new[] { unitId }))
            {
                return Forbid();
            }
            this.ValidateUnitHeadOrInCharged(unitId, masterData, shiftService, redis);

            scheduleService.ClearTempSections(unitId);
            bgJob.ClearPendingSectionUpdate(unitId, redis);

            return Ok();
        }

        [HttpGet("{unitId}")]
        public IActionResult GetSchedule(int unitId, int? originUnit = null)
        {
            if (!User.IsInRole(Roles.HeadNurse) &&
                (!originUnit.HasValue || !this.CheckUnitHeadOrInCharged(originUnit.Value, masterData, shiftService, redis)) &&
                !auth.VerifyUnit(User, new[] { unitId }))
            {
                return Forbid();
            }

            var schedule = scheduleService.GetSchedule(unitId);
            var reschedules = scheduleService.GetReschedules(unitId);
            ScheduleResultViewModel result = mapper.Map<ScheduleResultViewModel>(schedule);
            result.Reschedules = mapper.Map<IEnumerable<ScheduleViewModel>>(reschedules);
            var patients = schedule.Sections.Aggregate(new List<ScheduleSlot>() as IEnumerable<ScheduleSlot>,
                (a, b) => a.Concat(b.Slots)).Aggregate(new List<Patient>() as IEnumerable<Patient>,
                   (a, b) => a.Concat(b.PatientList.Select(x => x.Patient)));

            patients = reschedules.Select(x => x.Patient).Concat(patients).GroupBy(x => x.Id).Select(g => g.First());

            // doctor patient access
            bool filterOnlyDoctorPatient = this.IsDoctorAndSeeOwnPatientOnly(setting.Value);
            if (filterOnlyDoctorPatient)
            {
                var doctorId = User.GetUserIdAsGuid();
                var hideInfoList = patients.Where(x => x.DoctorId != doctorId).ToList();
                patients = patients.Where(x => x.DoctorId == doctorId).ToList();
                foreach (var item in hideInfoList)
                {
                    (patients as List<Patient>).Add(new Patient
                    {
                        Id = item.Id,
                        Name = item.Name,
                        UnitId = item.UnitId,
                        DoctorId = item.DoctorId,
                    });
                }
            }

            result.Patients = mapper.Map<IEnumerable<PatientViewModel>>(patients);
            if (result.UnitId == 0)
            {
                result.UnitId = unitId;
            }

            return Ok(result);
        }

        [HttpPost("{unitId}/slots")]
        public IActionResult SlotPatient(int unitId, [FromBody] SectionSlotPatientViewModel patientSlot)
        {
            if (!auth.VerifyUnit(User, new[] { unitId }))
            {
                return Forbid();
            }
            var sections = scheduleService.GetSections(unitId);
            if (!sections.Any(x => x.Id == patientSlot.SectionId))
            {
                return Forbid();
            }
            ValidateDoctorPatientModify(patientSlot.PatientId);

            int count = scheduleService.SlotPatientSchedule(patientSlot.SectionId, patientSlot.Slot, patientSlot.PatientId);

            if (count == 0)
            {
                return NotFound();
            }

            return Ok();
        }

        [HttpPost("slots/swap")]
        public IActionResult SwapSlotPatient([FromBody] SwapSlotViewModel request)
        {
            var sectionFirst = scheduleService.GetSection(request.first.SectionId);
            var sectionSecond = request.first.SectionId != request.second.SectionId ?
                scheduleService.GetSection(request.second.SectionId) : sectionFirst;

            if (!auth.VerifyUnit(User, new[] { sectionFirst.UnitId, sectionSecond.UnitId }))
            {
                return Forbid();
            }

            ValidateDoctorPatientModify(request.first.PatientId, request.second.PatientId);

            var first = mapper.Map<SectionSlotPatient>(request.first);
            var second = mapper.Map<SectionSlotPatient>(request.second);

            bool result = scheduleService.SwapSlot(first, second);

            if (!result)
            {
                return NotFound();
            }

            return Ok();
        }

        [HttpDelete("slots/{patientId}/{sectionId}/{slot}")]
        public IActionResult DeletePatientSlot(string patientId, int sectionId, SectionSlots slot)
        {
            var section = scheduleService.GetSection(sectionId);
            if (!auth.VerifyUnit(User, new[] { section.UnitId }))
            {
                return Forbid();
            }
            ValidateDoctorPatientModify(patientId);

            bool result = scheduleService.DeletePatientSlot(patientId, sectionId, slot);

            if (!result)
            {
                return NotFound();
            }

            return Ok();
        }

        [HttpPost("reschedule/{patientId}/{sectionId}/{slot}")]
        public IActionResult Reschedule(string patientId, int sectionId, SectionSlots slot, [FromBody] RescheduleViewModel reschedule)
        {
            var patient = patientService.GetPatient(patientId);
            if (!auth.VerifyUnit(User, new[] { patient.UnitId, reschedule.OverrideUnitId ?? patient.UnitId }))
            {
                return Forbid();
            }
            ValidateDoctorPatientModify(patient);

            if (patient.UnitId == reschedule.OverrideUnitId)
            {
                return BadRequest("Override unit cannot be the same with patient's unit.");
            }

            if (!string.IsNullOrWhiteSpace(reschedule.TargetPatientId) && !reschedule.OriginalDate.HasValue)
            {
                return BadRequest("OriginalDate cannot be null when TargetPatientId is specified.");
            }

            List<Schedule> schedules = new List<Schedule>();
            schedules.Add(new Schedule
            {
                PatientId = patientId,
                SectionId = sectionId,
                Slot = slot,
                Date = reschedule.Date.UtcDateTime,
                OverrideUnitId = reschedule.OverrideUnitId,
                OriginalDate = reschedule.OriginalDate?.UtcDateTime
            });

            if (reschedule.TargetPatientId != null)
            {
                schedules.Add(new Schedule
                {
                    PatientId = reschedule.TargetPatientId,
                    Date = reschedule.OriginalDate.Value.UtcDateTime,
                    OverrideUnitId = reschedule.OverrideUnitId.HasValue ? patient.UnitId : (int?)null,
                    OriginalDate = reschedule.Date.UtcDateTime,
                    Patient = new Patient { UnitId = reschedule.OverrideUnitId ?? patient.UnitId }
                });
            }

            var result = scheduleService.Reschedule(schedules);

            return Ok(result.Select(x => x.Id).ToList());
        }

        [HttpGet("{unitId}/active-schedule")]
        public IActionResult GetActiveScheduledPatient(int unitId)
        {
            if (!auth.VerifyUnit(User, new[] { unitId }))
            {
                return Forbid();
            }

            Expression<Func<Schedule, bool>> whereCondition = null;
            if (this.IsDoctorAndSeeOwnPatientOnly(setting.Value))
            {
                var doctorId = User.GetUserIdAsGuid();
                whereCondition = x => x.Patient.DoctorId == doctorId;
            }

            var schedules = scheduleService.GetActiveSchedules(unitId, whereCondition);

            return Ok(mapper.Map<IEnumerable<SchedulePatientViewModel>>(schedules));
        }

        [HttpDelete("reschedule/{scheduleId}")]
        public IActionResult DeleteSchedule(Guid scheduleId)
        {
            var schedule = scheduleService.GetPatientScheduleById(scheduleId);
            if (schedule == null)
            {
                return NotFound();
            }

            if (!auth.VerifyUnit(User, new[] { schedule.Section.UnitId }))
            {
                return Forbid();
            }

            ValidateDoctorPatientModify(schedule.PatientId);

            bool result = scheduleService.DeleteSchedule(scheduleId);

            if (!result)
            {
                return NotFound();
            }

            return Ok();
        }

        [HttpGet("{unitId}/is-empty")]
        public IActionResult CheckEmpty(int unitId)
        {
            bool result = scheduleService.IsScheduleEmpty(unitId);
            return Ok(result);
        }


        [HttpGet("{unitId}/max-per-slot")]
        public IActionResult GetMaxPerSlot(int unitId)
        {
            int defaultValue = unitSetting.Value.MaxPatientPerSlot.Value;

            var targetUnit = unitSetting.Get(unitId.ToString());
            if (targetUnit == null) return NotFound();

            if (targetUnit.MaxPatientPerSlot == 0)
            {
                return Ok(defaultValue);
            }

            return Ok(targetUnit.MaxPatientPerSlot);
        }

        [PermissionAuthorize(Permissions.UNIT_SETTING)]
        [HttpPut("{unitId}/max-per-slot/set/{value}")]
        public IActionResult SetMaxPerSlot(int unitId, int value)
        {
            if (!auth.VerifyUnit(User, new[] { unitId }))
            {
                return Forbid();
            }
            this.ValidateUnitHeadOrInCharged(unitId, masterData, shiftService, redis);

            unitSetting.Update(unitId.ToString(), x => x.MaxPatientPerSlot = value);

            return Ok();
        }

        // ========================= Settings =============================
        [HttpGet("setting")]
        public IActionResult GetSetting()
        {
            return Ok(setting.Value.Schedule);
        }

        [PermissionAuthorize(Permissions.SCHEDULE)]
        [HttpPut("setting")]
        public IActionResult SetSetting(ScheduleSetting request)
        {
            setting.Update(x =>
            {
                x.Schedule = request;
            });

            return Ok();
        }

        // ======================== Utils ============================
        private void ValidateDoctorPatientModify(params string[] patientId)
        {
            if (this.IsDoctor())
            {
                foreach (var item in patientId)
                {
                    var patient = patientService.GetPatient(item);
                    ValidateDoctorPatientModify(patient, true);
                }
            }
        }
        private void ValidateDoctorPatientModify(Patient patient, bool isDoctor = false)
        {
            if ((isDoctor || this.IsDoctor()) && patient.DoctorId != User.GetUserIdAsGuid())
            {
                throw new UnauthorizedException();
            }
        }
    }
}
