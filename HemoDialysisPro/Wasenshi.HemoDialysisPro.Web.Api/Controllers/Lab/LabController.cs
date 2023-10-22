using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Wasenshi.AuthPolicy.Attributes;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Services.BusinessLogic;
using Wasenshi.HemoDialysisPro.Services.Interfaces;
using Wasenshi.HemoDialysisPro.Share;
using Wasenshi.HemoDialysisPro.ViewModels;
using Wasenshi.HemoDialysisPro.ViewModels.Base;
using Wasenshi.HemoDialysisPro.Web.Api.Utils;
using Wasenshi.HemoDialysisPro.Web.Api.Utils.ModelSearchers;
using Wasenshi.HemoDialysisPro.Utils;
using Wasenshi.HemoDialysisPro.Models.ConfigExtesions;
using Wasenshi.HemoDialysisPro.Models.Settings;
using Wasenshi.AuthPolicy;

namespace Wasenshi.HemoDialysisPro.Web.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LabController : PatientBaseController
    {
        private readonly ILabService labService;
        private readonly IWritableOptions<GlobalSetting> setting;
        private readonly IMapper mapper;

        public LabController(
            ILabService labService,
            IPatientService patientService,
            IVerifyPatientService verifyPatientService,
            IScheduleService scheduleService,
            IWritableOptions<GlobalSetting> setting,
            IMapper mapper) : base(patientService, verifyPatientService, scheduleService)
        {
            this.labService = labService;
            this.setting = setting;
            this.mapper = mapper;
        }

        [HttpGet]
        public IActionResult GetAllLabExams(int page = 1, int limit = 1000, [FromQuery] List<string> orderBy = null, [FromQuery] DateTimeOffset? filter = null)
        {
            Action<IOrderer<LabExam>> orders = null;
            if (orderBy?.Count > 0)
            {
                orders = (orderer) =>
                {
                    foreach (var item in orderBy)
                    {
                        var split = item.Split('_');
                        string key = split[0];
                        bool desc = split.Length > 1 && split[1] == "desc";
                        Order(key, desc, orderer);
                    }
                    void Order(string key, bool desc, IOrderer<LabExam> orderer)
                    {
                        switch (key.ToLower())
                        {
                            case "id":
                                orderer.OrderBy((h) => h.Id, desc);
                                break;
                            case "date":
                                orderer.OrderBy((h) => h.EntryTime, desc);
                                break;
                            case "value":
                                orderer.OrderBy((h) => h.LabValue, desc);
                                break;
                            case "name":
                                orderer.OrderBy((h) => h.LabItem.Name, desc);
                                break;
                        }
                    }
                };
            }
            else
            {
                // Default ordering (patient then name then date)
                orders = (orderer) => orderer
                .OrderBy(x => x.PatientId)
                .OrderBy(x => x.LabItem.Name)
                .OrderBy(x => x.EntryTime, true);
            }

            Expression<Func<LabExam, bool>> whereCondition = null;
            DateTime limitDate;
            if (!filter.HasValue)
            {
                // Default limit within 3 months
                limitDate = DateTime.UtcNow.AddMonths(-2);
                limitDate = new DateTime(limitDate.Year, limitDate.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            }
            else
            {
                limitDate = filter.Value.ToUtcDate();
            }
            whereCondition = x => x.EntryTime > limitDate;
            if (this.IsDoctorAndSeeOwnPatientOnly(setting.Value))
            {
                var doctorId = User.GetUserIdAsGuid();
                whereCondition = whereCondition.AndAlso(x => x.Patient.DoctorId == doctorId);
            }

            Page<LabExam> result = labService.GetAllLabExams(page, limit, orders, whereCondition);

            var count = result.Total;
            var data = mapper.Map<IEnumerable<LabExam>, IEnumerable<LabExamViewModel>>(result.Data);

            return Ok(new PageView<LabExamViewModel>
            {
                Data = data,
                Total = count
            });
        }

        [HttpPost]
        public IActionResult CreateLabExamBatch(CreateLabExamBatchViewModel request)
        {
            var patient = FindPatient(request.PatientId);
            if (patient == null)
            {
                return NotFound();
            }

            ValidateDoctorModify(patient);

            IEnumerable<LabExam> newLabs = mapper.Map<IEnumerable<LabExam>>(request.LabExams);

            IEnumerable<LabExam> result;
            try
            {
                result = labService.CreateLabExamBatch(request.PatientId, request.EntryTime.UtcDateTime, newLabs.ToList());
            }
            catch (SystemBoundException e)
            {
                return BadRequest(new
                {
                    e.Errors
                });
            }

            return Ok(mapper.Map<IEnumerable<LabExam>, IEnumerable<LabExamViewModel>>(result));
        }

        [HttpPost("{id}")]
        public IActionResult UpdateLabExam(Guid id, LabExamViewModel labExam)
        {
            var patient = FindPatient(labExam.PatientId);
            if (patient == null)
            {
                return NotFound();
            }

            ValidateDoctorModify(patient);

            labExam.Id = id;
            LabExam updateLab = mapper.Map<LabExam>(labExam);

            LabExam result;
            try
            {
                result = labService.UpdateLabExam(updateLab);
            }
            catch (SystemBoundException e)
            {
                return BadRequest(e.Message);
            }

            return Ok(mapper.Map<LabExam, LabExamViewModel>(result));
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteLabExam(Guid id)
        {
            var labExam = labService.GetLabExam(id);
            var patient = FindPatient(labExam.PatientId);
            if (patient == null)
            {
                return NotFound();
            }

            ValidateDoctorModify(patient);

            var result = labService.DeleteLabExam(id);
            if (result)
            {
                return Ok();
            }

            return NotFound();
        }

        [HttpGet("{id}")]
        public IActionResult GetLabExam(Guid id)
        {
            var result = labService.GetLabExam(id);
            if (result == null)
            {
                return NotFound();
            }

            ValidateDoctorAccess(result.Patient);

            return Ok(mapper.Map<LabExam, LabExamViewModel>(result));
        }

        [HttpGet("patient/{patientId}")]
        public IActionResult GetAllLabExamByPatientId(string patientId, [FromQuery] double? filter = null, [FromQuery] double? upperLimit = null)
        {
            DateTime? lowerD = filter?.UnixTimeStampToDateTime();
            DateTime? upperD = upperLimit?.UnixTimeStampToDateTime();
            LabExamResult recordResult = GetLabExamData(patientId, lowerD, upperD);
            if (recordResult == null)
            {
                return NotFound();
            }

            return Ok(mapper.Map<LabExamResult, LabExamResultViewModel>(recordResult));
        }

        [HttpGet("patient/overview")]
        public IActionResult GetAllPatientWithLabOverview(int page = 1, int limit = 25, [FromQuery] List<string> orderBy = null, string where = null)
        {
            Action<IOrderer<LabOverview>> orders = null;
            if (orderBy?.Count > 0)
            {
                orders = orderer =>
                {
                    foreach (var item in orderBy)
                    {
                        var split = item.Split('_');
                        string key = split[0];
                        bool desc = split.Length > 1 && split[1] == "desc";
                        Order(key, desc, orderer);
                    }
                    void Order(string key, bool desc, IOrderer<LabOverview> orderer)
                    {
                        switch (key.ToLower())
                        {
                            case "id":
                                orderer.OrderBy(l => l.Patient.Id, desc);
                                break;
                            case "hn":
                                orderer.OrderBy(l => l.Patient.HospitalNumber, desc);
                                break;
                            case "name":
                                orderer.OrderBy(l => l.Patient.Name, desc);
                                break;
                            case "gender":
                                orderer.OrderBy(l => l.Patient.Gender, desc);
                                break;
                            case "unit":
                                orderer.OrderBy(l => l.Patient.UnitId, desc);
                                break;
                            case "last":
                                orderer.OrderBy(l => l.LastRecord, desc);
                                break;
                            case "total":
                                orderer.OrderBy(l => l.Total, desc);
                                break;
                        }
                    }
                };
            }

            Expression<Func<LabOverview, bool>> whereCondition = null;
            if (!string.IsNullOrWhiteSpace(where))
            {
                whereCondition = new LabOverviewSearch().GetWhereCondition(where);
                if (whereCondition == null)
                {
                    return Ok(new PageView<LabOverviewViewModel>
                    {
                        Data = Enumerable.Empty<LabOverviewViewModel>(),
                        Total = 0
                    });
                }
            }
            //Page<LabOverview> labOverviews = null;

            Expression<Func<LabOverview, bool>> unitFilter = User.GetUnitFilter<LabOverview>(x => x.Patient.UnitId);
            whereCondition = unitFilter.AndAlso(whereCondition);
            if (this.IsDoctorAndSeeOwnPatientOnly(setting.Value))
            {
                var doctorId = User.GetUserIdAsGuid();
                whereCondition = whereCondition.AndAlso(x => x.Patient.DoctorId == doctorId);
            }

            Page<LabOverview> labOverviews = labService.GetLabOverview(page, limit, orders, whereCondition);

            var count = labOverviews.Total;
            var data = mapper.Map<IEnumerable<LabOverview>, IEnumerable<LabOverviewViewModel>>(labOverviews.Data);

            return Ok(new PageView<LabOverviewViewModel>
            {
                Data = data,
                Total = count
            });
        }

        // =================== Setting ================================

        [HttpGet("hemosheet")]
        public IActionResult GetLabHemosheet()
        {
            var result = labService.GetLabHemosheetList();
            if (result == null)
            {
                return NotFound();
            }

            return Ok(mapper.Map<IEnumerable<LabHemosheet>, IEnumerable<LabHemosheetViewModel>>(result));
        }

        [PermissionAuthorize(Permissions.Hemosheet.SETTING)]
        [HttpPost("hemosheet")]
        public IActionResult UpdateLabHemosheet(LabHemosheetUpdateViewModel request)
        {
            var list = mapper.Map<IEnumerable<LabHemosheet>>(request.List);
            labService.AddOrUpdateLabHemosheet(list);

            return Ok();
        }

        private LabExamResult GetLabExamData(string patientId, DateTime? filter, DateTime? upperLimit = null)
        {
            var patient = PatientService.GetPatient(patientId);
            if (patient == null)
            {
                return null;
            }

            ValidateDoctorModify(patient);

            Expression<Func<LabExam, bool>> unitFilter = User.GetUnitFilter<LabExam>(x => patient.UnitId);
            LabExamResult recordResult = labService.GetLabExamByPatientId(patientId, unitFilter, filter, upperLimit);

            recordResult.Patient = patient;

            return recordResult;
        }

        private void ValidateDoctorModify(Patient patient)
        {
            if (this.IsDoctor())
            {
                var doctorId = User.GetUserIdAsGuid();
                if (doctorId != patient.DoctorId)
                {
                    throw new UnauthorizedException();
                }
            }
        }

        private void ValidateDoctorAccess(Patient patient)
        {
            if (this.IsDoctorAndSeeOwnPatientOnly(setting.Value))
            {
                var doctorId = User.GetUserIdAsGuid();
                if (doctorId != patient.DoctorId)
                {
                    throw new UnauthorizedException();
                }
            }
        }
    }
}
