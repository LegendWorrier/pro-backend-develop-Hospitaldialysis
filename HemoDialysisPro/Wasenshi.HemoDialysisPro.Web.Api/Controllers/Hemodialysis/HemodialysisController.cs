using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using ServiceStack.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Web;
using Wasenshi.AuthPolicy;
using Wasenshi.AuthPolicy.Attributes;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Models.Settings;
using Wasenshi.HemoDialysisPro.Services.Core.BusinessLogic;
using Wasenshi.HemoDialysisPro.Services.Interfaces;
using Wasenshi.HemoDialysisPro.Share;
using Wasenshi.HemoDialysisPro.Models.ConfigExtesions;
using Wasenshi.HemoDialysisPro.Models.TimezoneUtil;
using Wasenshi.HemoDialysisPro.ViewModels;
using Wasenshi.HemoDialysisPro.ViewModels.Base;
using Wasenshi.HemoDialysisPro.Web.Api.Controllers.Utils;
using Wasenshi.HemoDialysisPro.Web.Api.Utils;
using Wasenshi.HemoDialysisPro.Web.Api.Utils.ModelSearchers;
using Wasenshi.HemoDialysisPro.Utils;
using Wasenshi.HemoDialysisPro.Services;
using Wasenshi.HemoDialysisPro.Repositories.Interfaces;
using Wasenshi.HemoDialysisPro.Services.Core.Interfaces;
using Wasenshi.HemoDialysisPro.PluginBase;
using Wasenshi.HemoDialysisPro.PluginIntegrate;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

namespace Wasenshi.HemoDialysisPro.Web.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HemodialysisController : PatientBaseController
    {
        private readonly IHemoService hemoService;
        private readonly ICosignService cosignService;
        private readonly IRecordService recordService;
        private readonly IShiftService shiftService;
        private readonly IShiftUnitOfWork shiftUow;
        private readonly IUserInfoService userInfoService;
        private readonly IMasterDataService masterData;
        private readonly IEnumerable<IDocumentHandler> docPlugins;
        private readonly IMapper mapper;
        private readonly IWritableOptions<GlobalSetting> setting;
        private readonly IRedisClient redis;
        private readonly IConfiguration config;

        private readonly TimeZoneInfo tz;

        private readonly IServiceProvider sp;

        public HemodialysisController(
            IHemoService hemoService,
            ICosignService cosignService,
            IRecordService recordService,
            IPatientService patientService,
            IVerifyPatientService verifyPatientService,
            IScheduleService scheduleService,
            IShiftService shiftService,
            IShiftUnitOfWork shiftUow,
            IUserInfoService userInfoService,
            IMasterDataService masterData,
            IEnumerable<IDocumentHandler> docPlugins,
            IServiceProvider sp,
            IMapper mapper,
            IWritableOptions<GlobalSetting> setting,
            IRedisClient redis,
            IConfiguration config) : base(patientService, verifyPatientService, scheduleService)
        {
            this.hemoService = hemoService;
            this.cosignService = cosignService;
            this.recordService = recordService;
            this.shiftService = shiftService;
            this.shiftUow = shiftUow;
            this.userInfoService = userInfoService;
            this.masterData = masterData;
            this.docPlugins = docPlugins;
            this.sp = sp;
            this.mapper = mapper;
            this.setting = setting;
            this.redis = redis;
            this.config = config;
            tz = TimezoneUtils.GetTimeZone(config["TIMEZONE"]);
        }

        [HttpGet("records")]
        public IActionResult GetAll(int page = 1, int limit = 25, [FromQuery] List<string> orderBy = null, string where = null)
        {
            Action<IOrderer<HemoRecordResult>> orders = null;
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
                    void Order(string key, bool desc, IOrderer<HemoRecordResult> orderer)
                    {
                        switch (key.ToLower())
                        {
                            case "id":
                                orderer.OrderBy((h) => h.Record.Id, desc);
                                break;
                            case "patient":
                                orderer.OrderBy((h) => h.Record.PatientId, desc);
                                break;
                            case "date":
                                orderer.OrderBy((h) => h.Record.CompletedTime, desc).OrderBy(h => h.Record.Created);
                                break;
                            case "ward":
                                orderer.OrderBy((h) => h.Record.Ward, desc);
                                break;
                        }
                    }
                };
            }

            Expression<Func<HemoRecordResult, bool>> whereCondition = null;
            if (!string.IsNullOrWhiteSpace(where))
            {
                whereCondition = new HemoResultSearch(tz).GetWhereCondition(where);
                if (whereCondition == null)
                {
                    return Ok(new PageView<HemoResultViewModel>
                    {
                        Data = Enumerable.Empty<HemoResultViewModel>(),
                        Total = 0
                    });
                }
            }
            if (this.IsDoctorAndSeeOwnPatientOnly(setting.Value))
            {
                var doctorId = User.GetUserIdAsGuid();
                Expression<Func<HemoRecordResult, bool>> doctorPatient = x => x.Patient.DoctorId == doctorId;
                whereCondition = doctorPatient.AndAlso(whereCondition);
            }

            Expression<Func<HemoRecordResult, bool>> unitFilter = User.GetUnitFilter<HemoRecordResult>(x => x.Patient.UnitId);
            whereCondition = unitFilter.AndAlso(whereCondition);
            Page<HemoRecordResult> result = hemoService.GetAllHemodialysisRecords(page, limit, orders, whereCondition);

            var count = result.Total;
            var data = mapper.Map<IEnumerable<HemoRecordResult>, IEnumerable<HemoResultViewModel>>(result.Data);

            return Ok(new PageView<HemoResultViewModel>
            {
                Data = data,
                Total = count
            });
        }

        [HttpGet("records/patient/{patientId}/with-note")]
        public IActionResult GetAllForPatientWithNote(string patientId, int page = 1, int limit = 25, [FromQuery] List<string> orderBy = null, string where = null)
        {
            var patient = FindPatient(patientId);
            if (patient == null)
            {
                return NotFound();
            }

            ValidateDoctorPatientAccess(patient);

            Action<IOrderer<HemodialysisRecord>> orders = null;
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
                    void Order(string key, bool desc, IOrderer<HemodialysisRecord> orderer)
                    {
                        switch (key.ToLower())
                        {
                            case "id":
                                orderer.OrderBy((h) => h.Id, desc);
                                break;
                            case "patient":
                                orderer.OrderBy((h) => h.PatientId, desc);
                                break;
                            case "date":
                                orderer.OrderBy((h) => h.CompletedTime, desc).OrderBy(h => h.Created);
                                break;
                            case "ward":
                                orderer.OrderBy((h) => h.Ward, desc);
                                break;
                        }
                    }
                };
            }
            Expression<Func<HemodialysisRecord, bool>> whereCondition = null;
            if (!string.IsNullOrWhiteSpace(where))
            {
                whereCondition = new HemoSearch(tz).GetWhereCondition(where);
                if (whereCondition == null)
                {
                    return Ok(new PageView<HemodialysisRecordViewModel>
                    {
                        Data = Enumerable.Empty<HemodialysisRecordViewModel>(),
                        Total = 0
                    });
                }
            }

            Expression<Func<HemodialysisRecord, bool>> filter = x => x.PatientId == patientId && x.CompletedTime != null;
            whereCondition = filter.AndAlso(whereCondition);

            Page<HemodialysisRecord> records = hemoService.GetAllHemodialysisRecordsWithNote(page, limit, orders, whereCondition);

            return Ok(new PageView<HemodialysisRecordViewModel>
            {
                Data = mapper.Map<IEnumerable<HemodialysisRecord>, IEnumerable<HemodialysisRecordViewModel>>(records.Data),
                Total = records.Total
            });
        }

        [HttpGet("records/patient/{patientId}")]
        public IActionResult GetAllRecordsForPatient(string patientId, int page = 1, int limit = 25, string where = null)
        {
            patientId = HttpUtility.UrlDecode(patientId);
            var patient = FindPatient(patientId);
            if (patient == null)
            {
                return NotFound();
            }

            ValidateDoctorPatientAccess(patient);

            Expression<Func<HemodialysisRecord, bool>> whereCondition = null;
            if (!string.IsNullOrWhiteSpace(where))
            {
                whereCondition = new HemoSearch(tz).GetWhereCondition(where);
                if (whereCondition == null)
                {
                    return Ok(new PageView<HemodialysisRecordViewModel>
                    {
                        Data = Enumerable.Empty<HemodialysisRecordViewModel>(),
                        Total = 0
                    });
                }
            }

            Page<HemodialysisRecord> records = hemoService.GetHemodialysisRecordsByPatientId(patientId, page, limit, whereCondition);

            return Ok(new PageView<HemodialysisRecordViewModel>
            {
                Data = mapper.Map<IEnumerable<HemodialysisRecord>, IEnumerable<HemodialysisRecordViewModel>>(records.Data),
                Total = records.Total
            });
        }

        [HttpGet("records/patient/{patientId}/count-info")]
        public IActionResult GetRecordsCountInfoForPatient(string patientId)
        {
            patientId = HttpUtility.UrlDecode(patientId);
            var patient = FindPatient(patientId);
            if (patient == null)
            {
                return NotFound();
            }

            ValidateDoctorPatientAccess(patient);

            int countAll = hemoService.CountAll(x => x.Patient.Id == patientId && x.Record.CompletedTime.HasValue);
            var tzNow = TimeZoneInfo.ConvertTime(new DateTimeOffset(DateTime.UtcNow, TimeSpan.Zero), tz).AsDate();
            int countThisMonth = hemoService.CountAll(x => x.Patient.Id == patientId &&
                x.Record.CompletedTime.Value.Year == tzNow.Year && x.Record.CompletedTime.Value.Month == tzNow.Month);

            return Ok(new
            {
                Total = countAll,
                ThisMonth = countThisMonth
            });
        }

        [HttpGet("prescriptions/patient/{patientId}")]
        public IActionResult GetAllPrescriptionForPatient(string patientId)
        {
            patientId = HttpUtility.UrlDecode(patientId);
            var patient = FindPatient(patientId);
            if (patient == null)
            {
                return NotFound();
            }

            ValidateDoctorPatientAccess(patient);

            IEnumerable<DialysisPrescription> records = hemoService.GetDialysisPrescriptionsByPatientId(patientId);

            return Ok(mapper.Map<IEnumerable<DialysisPrescription>, IEnumerable<DialysisPrescriptionViewModel>>(records));
        }

        [HttpPost("prescriptions")]
        public async Task<IActionResult> CreatePrescription([FromBody] EditDialysisPrescriptionViewModel prescription)
        {
            DialysisPrescription newPrescription = mapper.Map<DialysisPrescription>(prescription);

            var patient = PatientService.GetPatient(prescription.PatientId);
            if (patient == null)
            {
                return NotFound();
            }
            await this.ValidateResourcePermissionAsync(patient);

            if (!VerifyPatienService.VerifyUnit(User, patient))
            {
                return Forbid();
            }

            if (setting.Value.Hemosheet.Rules.DialysisPrescriptionRequireHeadNurse)
            {
                var unitId = patient.UnitId;
                this.ValidateUnitHeadOrInCharged(unitId, masterData, shiftService, redis);
            }

            hemoService.CreatePrescription(newPrescription);

            return Created($"{newPrescription.Id}", mapper.Map<DialysisPrescriptionViewModel>(newPrescription));
        }

        [HttpPost("records")]
        public async Task<IActionResult> CreateHemoSheetAsync([FromBody] EditHemodialysisRecordViewModel record)
        {
            HemodialysisRecord newHemoRecord = mapper.Map<HemodialysisRecord>(record);
            var patient = PatientService.GetPatient(record.PatientId);
            if (patient == null)
            {
                return NotFound();
            }
            await this.ValidateResourcePermissionAsync(patient);

            if (!VerifyPatienService.VerifyUnit(User, patient))
            {
                return Forbid();
            }

            // inject current shift's section of patient's unit
            var shiftInfo = redis.GetUnitShift(patient.UnitId);

            hemoService.CreateHemodialysisRecord(newHemoRecord, shiftInfo?.CurrentSection);

            var lastCompleted = hemoService.GetPreviousHemosheet(newHemoRecord);
            if (lastCompleted != null)
            {
                newHemoRecord.Dehydration.LastPostWeight = lastCompleted.Dehydration.PostWeight();
            }

            return Created($"{newHemoRecord.Id}", mapper.Map<HemodialysisRecordViewModel>(newHemoRecord));
        }

        [HttpPost("records/{id}")]
        [FieldAuthorize]
        public async Task<IActionResult> EditHemoSheetAsync(Guid id, EditHemodialysisRecordViewModel record)
        {
            (var actionResult, HemodialysisRecord originalRecord, _) = await _ValidateAndGetHemoRecord(id, true);
            if (actionResult != null)
            {
                return actionResult;
            }

            HemodialysisRecord editHemoRecord = mapper.Map(record, originalRecord);
            editHemoRecord.Id = id;
            if (editHemoRecord.DialysisPrescription?.Id != editHemoRecord.DialysisPrescriptionId)
            {
                editHemoRecord.DialysisPrescription = null;
            }

            bool result = hemoService.EditHemodialysisRecord(editHemoRecord);

            if (!result)
            {
                return NotFound();
            }

            return Ok();
        }

        [HttpGet("records/{id}")]
        public IActionResult GetHemoSheet(Guid id)
        {
            HemodialysisRecord record = hemoService.GetHemodialysisRecord(id);
            if (record == null)
            {
                return NotFound();
            }
            // Verify hemosheet's Patient
            Patient patient = FindPatient(record.PatientId);
            if (patient == null)
            {
                return NotFound();
            }

            ValidateDoctorPatientAccess(patient);

            var result = mapper.Map<HemodialysisRecordViewModel>(record);
            var lastCompleted = hemoService.GetPreviousHemosheet(record);
            if (lastCompleted != null)
            {
                if ((result.Dehydration.LastPostWeight ?? 0) == 0)
                {
                    result.Dehydration.LastPostWeight = lastCompleted.Dehydration.PostWeight();
                }
            }

            return Ok(result);
        }

        [HttpGet("patient/{patientId}/hemosheet")]
        public IActionResult GetLatestHemoSheet(string patientId)
        {
            patientId = HttpUtility.UrlDecode(patientId);
            // Verify hemosheet's Patient
            Patient patient = FindPatient(patientId);
            if (patient == null)
            {
                return NotFound();
            }
            ValidateDoctorPatientAccess(patient);

            var record = hemoService.GetHemodialysisRecordByPatientId(patientId);
            HemodialysisRecord lastCompleted = hemoService.GetPreviousHemosheet(record);

            var result = mapper.Map<HemodialysisRecordViewModel>(record);
            if (result != null && lastCompleted != null)
            {
                if ((result.Dehydration.LastPostWeight ?? 0) == 0)
                {
                    result.Dehydration.LastPostWeight = lastCompleted.Dehydration.PostWeight();
                }
            }

            return Ok(result);
        }

        [HttpGet("patient/{patientId}/prescription/check")]
        public IActionResult CheckLatestDialysisPrescription(string patientId)
        {
            patientId = HttpUtility.UrlDecode(patientId);
            bool result = hemoService.CheckDialysisPrescriptionExists(patientId);

            return Ok(result);
        }

        [HttpGet("records/{id}/check-unexecuted")]
        public IActionResult CheckUnexecutedRecords(Guid id)
        {
            bool result = recordService.CheckAnyUnexecutedRecord(id);

            return Ok(result);
        }

        [HttpPost("records/{id}/complete")]
        public async Task<IActionResult> CompleteHemoSheetAsync(Guid id, [FromBody] CompleteHemoViewModel body)
        {
            await ValidateHemosheetForDoctorAsync(id);

            HemodialysisRecord data = null;
            if (body.Update != null || body.CompleteTime.HasValue)
            {
                data = mapper.Map<HemodialysisRecord>(body.Update);
                if (body.CompleteTime.HasValue)
                {
                    var requirePermission = setting.Value.Hemosheet.Rules.ChangeCompleteTimePermissionRequired;
                    if (requirePermission && !User.IsInAnyRole(new[] { Roles.Admin, Roles.HeadNurse }))
                    {
                        return Forbid();
                    }
                    data.CompletedTime = body.CompleteTime.Value.UtcDateTime;
                }
            }
            bool result = hemoService.CompleteHemodialysisRecord(id, data);
            if (!result)
            {
                return NotFound();
            }

            return Ok();
        }

        [HttpPost("records/{id}/change-complete-time")]
        public async Task<IActionResult> ChangeCompleteTimeAsync(Guid id, CompleteHemoViewModel request)
        {
            await ValidateHemosheetForDoctorAsync(id);

            var requirePermission = setting.Value.Hemosheet.Rules.ChangeCompleteTimePermissionRequired;
            if (requirePermission && !User.IsInAnyRole(new[] { Roles.Admin, Roles.HeadNurse }))
            {
                return Forbid();
            }

            if (!request.CompleteTime.HasValue)
            {
                return BadRequest();
            }

            var result = hemoService.ChangeCompleteTime(id, request.CompleteTime.Value);
            if (!result)
            {
                return NotFound();
            }

            return Ok();
        }

        [PermissionAuthorize(Permissions.Hemosheet.DELETE)]
        [HttpDelete("records/{id}")]
        public async Task<IActionResult> DeleteHemoSheetAsync(Guid id)
        {
            (IActionResult getResult, HemodialysisRecord hemo, _) = await _ValidateAndGetHemoRecord(id, true);
            if (getResult != null)
            {
                return getResult;
            }
            bool result = hemoService.DeleteHemosheet(id);
            if (!result)
            {
                return NotFound();
            }

            if (redis.IsInSession(hemo.PatientId) && hemo.CompletedTime == null)
            {
                redis.RemoveSession(hemo);
            }

            return Ok();
        }

        [HttpPost("prescriptions/{id}")]
        public async Task<IActionResult> EditPrescriptionAsync(Guid id, EditDialysisPrescriptionViewModel prescription)
        {
            // Verify prescription's Patient
            Patient patient = FindPatient(prescription.PatientId);
            if (patient == null)
            {
                return Forbid();
            }

            if (setting.Value.Hemosheet.Rules.DialysisPrescriptionRequireHeadNurse)
            {
                this.ValidateUnitHeadOrInCharged(patient.UnitId, masterData, shiftService, redis);
            }

            await this.ValidateResourcePermissionAsync(patient);

            prescription.Id = id;
            DialysisPrescription editPrescription = mapper.Map<DialysisPrescription>(prescription);

            bool result = hemoService.EditPrescription(editPrescription);

            if (!result)
            {
                return NotFound();
            }

            return Ok();
        }

        [HttpGet("prescriptions/{id}")]
        public IActionResult GetPrescription(Guid id)
        {
            DialysisPrescription prescription = hemoService.GetDialysisPrescription(id);
            if (prescription == null)
            {
                return NotFound();
            }
            // Verify prescription's Patient
            Patient patient = FindPatient(prescription.PatientId);
            if (patient == null)
            {
                return NotFound();
            }
            ValidateDoctorPatientAccess(patient);
            return Ok(mapper.Map<DialysisPrescriptionViewModel>(prescription));
        }

        [HttpDelete("prescriptions/{id}")]
        public async Task<IActionResult> DeletePrescriptionAsync(Guid id)
        {
            DialysisPrescription prescription = hemoService.GetDialysisPrescription(id);
            if (prescription == null)
            {
                return NotFound("Prescription not found.");
            }

            Patient patient = PatientService.GetPatient(prescription.PatientId);

            if (patient == null)
            {
                return NotFound("Patient not found.");
            }
            if (setting.Value.Hemosheet.Rules.DialysisPrescriptionRequireHeadNurse)
            {
                this.ValidateUnitHeadOrInCharged(patient.UnitId, masterData, shiftService, redis);
            }

            await this.ValidateResourcePermissionAsync(patient);

            hemoService.DeletePrescription(id);

            return Ok();
        }

        [Authorize(Roles = Roles.NotDoctor)]
        [HttpPost("records/{id}/cosign")]
        public async Task<IActionResult> CoSignRequestHemosheet(Guid id, [FromBody] CosignRequestViewModel cosign)
        {
            if (string.IsNullOrWhiteSpace(cosign.Password))
            {
                return ValidationProblem("Password cannot be null");
            }

            bool result = await cosignService.AssignCosignForHemosheet(id, cosign.UserId, cosign.Password);
            if (!result)
            {
                return Forbid();
            }

            return Ok();
        }

        [Authorize(Roles = Roles.HeadNurseUp)]
        [HttpPut("records/{id}/doctor-consent")]
        public async Task<IActionResult> DoctorConsentHemosheet(Guid id)
        {
            bool headnursePermit = setting.Value.Hemosheet.Rules.HeadNurseCanApproveDoctorSignature;
            if (!User.IsInRole(Roles.Doctor) && !headnursePermit)
            {
                return Forbid();
            }
            (var actionResult, _,_) = await _ValidateAndGetHemoRecord(id);
            if (actionResult != null)
            {
                return actionResult;
            }

            bool result = hemoService.UpdateDoctorConsent(id);
            if (!result)
            {
                return Forbid();
            }

            return Ok();
        }

        [Authorize(Roles = Roles.HeadNurseUp)]
        [HttpPut("records/{id}/doctor-consent/revoke")]
        public async Task<IActionResult> DoctorConsentHemosheetRevoke(Guid id)
        {
            bool headnursePermit = setting.Value.Hemosheet.Rules.HeadNurseCanApproveDoctorSignature;
            if (!User.IsInRole(Roles.Doctor) && !headnursePermit)
            {
                return Forbid();
            }
            (var actionResult, _,_) = await _ValidateAndGetHemoRecord(id);
            if (actionResult != null)
            {
                return actionResult;
            }

            bool result = hemoService.UpdateDoctorConsent(id, false);
            if (!result)
            {
                return Forbid();
            }

            return Ok();
        }

        [Authorize(Roles = Roles.NotDoctor)]
        [HttpPut("records/{id}/claim")]
        public async Task<IActionResult> ClaimHemosheet(Guid id)
        {
            var result = hemoService.ClaimHemosheet(id, User.GetUserIdAsGuid());
            if (!result)
            {
                return BadRequest();
            }

            return Ok();
        }

        [Authorize(Roles = Roles.NotDoctor)]
        [HttpPost("records/{id}/nurses-in-shift")]
        public async Task<IActionResult> UpdateNurseInShift(Guid id, [FromBody] IEnumerable<Guid> list)
        {
            (var actionResult, _,_) = await _ValidateAndGetHemoRecord(id, true);
            if (actionResult != null)
            {
                return actionResult;
            }

            bool result = hemoService.UpdateNurseInShift(id, list);
            if (!result)
            {
                return NotFound();
            }

            return Ok();
        }

        /// <summary>
        /// This action will update the Nurse in Shift to current latest nurses list in the system calculated from hemosheet cycleStartTime
        /// and link to target Nurse Shifts.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Roles = Roles.NotDoctor)]
        [HttpPut("records/{id}/nurses-in-shift/update-current")]
        public async Task<IActionResult> UpdateNurseInShiftToLatestShiftInfo(Guid id)
        {
            (var actionResult, var hemosheet, var patient) = await _ValidateAndGetHemoRecord(id, true);
            if (actionResult != null)
            {
                return actionResult;
            }

            hemosheet.ShiftSectionId = 0;
            var list = hemosheet.GetNurseInShift(shiftUow, userInfoService, patient, tz, true);

            bool result = hemoService.UpdateNurseInShift(id, list);
            if (!result)
            {
                return NotFound();
            }

            return Ok(list);
        }

        [Authorize(Roles = Roles.NotDoctor)]
        [HttpPost("records/{id}/note")]
        public async Task<IActionResult> UpdateNote(Guid id, HemoNoteViewModel note)
        {
            (var actionResult, _,_) = await _ValidateAndGetHemoRecord(id, true);
            if (actionResult != null)
            {
                return actionResult;
            }

            var updateData = mapper.Map<HemoNote>(note);
            updateData.HemoId = id;

            var result = hemoService.UpdateHemoNote(updateData);
            if (result == null)
            {
                return NotFound();
            }

            return Ok(mapper.Map<HemoNoteViewModel>(result));
        }

        [Authorize(Roles = Roles.NotDoctor)]
        [HttpPost("prescriptions/{id}/nurse")]
        public async Task<IActionResult> NurseRequestPrescription(Guid id, [FromBody] CosignRequestViewModel nurse)
        {
            if (string.IsNullOrWhiteSpace(nurse.Password))
            {
                return ValidationProblem("Password cannot be null");
            }

            bool result = await cosignService.AssignNurseForDialysisPrescription(id, nurse.UserId, nurse.Password);
            if (!result)
            {
                return Forbid();
            }

            return Ok();
        }

        [Authorize(Roles = Roles.NotDoctor)]
        [HttpPost("prescriptions/{id}/nurse/self")]
        public async Task<IActionResult> ClaimNursePrescription(Guid id)
        {
            bool result = await cosignService.AssignNurseForDialysisPrescription(id, User.GetUserIdAsGuid());
            if (!result)
            {
                return Forbid();
            }

            return Ok();
        }

        [HttpPut("record/{id}/send-pdf")]
        public async Task<IActionResult> SendHemosheetPDF(Guid id)
        {
            (var actionResult, HemodialysisRecord hemosheet, _) = await _ValidateAndGetHemoRecord(id, true);
            if (actionResult != null)
            {
                return actionResult;
            }

            if (!docPlugins.Any())
            {
                return NotFound();
            }

            bool result = false;
            await docPlugins.ExecutePlugins(async (docHandler) =>
            {
                result = await docHandler.SendHemosheet(hemosheet);
            }, e => Log.Error(e, "Plugin error at sending hemosheet pdf"));

            if (!result)
            {
                return StatusCode(500, "Failed to send pdf. Please contact administrator.");
            }

            return Ok();
        }

        // ========================= Settings =============================
        [HttpGet("setting")]
        public IActionResult GetSetting()
        {
            HemosheetSetting hemosheetSetting = setting.Value.Hemosheet;
            hemosheetSetting.Basic ??= new();
            hemosheetSetting.Rules ??= new();

            return Ok(hemosheetSetting);
        }

        [PermissionAuthorize(Permissions.HEMOSHEET)]
        [HttpPut("setting")]
        public async Task<IActionResult> SetSettingAsync(HemosheetSetting request)
        {
            if (request.Rules != null)
            {
                await this.ValidatePermissionAsync(Permissions.Hemosheet.RULE, Permissions.GLOBAL);
            }
            else
            {
                request.Rules = setting.Value?.Hemosheet?.Rules;
            }
            if (request.Basic != null)
            {
                await this.ValidatePermissionAsync(Permissions.Hemosheet.SETTING, Permissions.GLOBAL);
            }
            else
            {
                request.Basic = setting.Value?.Hemosheet?.Basic;
            }

            setting.Update(x =>
            {
                x.Hemosheet = request;
            });

            return Ok();
        }

        // ===================== Util =========================

        private async Task<(IActionResult, HemodialysisRecord, Patient)> _ValidateAndGetHemoRecord(Guid recordId, bool allowNurse = false)
        {
            HemodialysisRecord originalRecord = hemoService.GetHemodialysisRecord(recordId);
            if (originalRecord == null)
            {
                return (NotFound(), null, null);
            }

            // Verify hemosheet's Patient
            Patient patient = FindPatient(originalRecord.PatientId);
            if (patient == null)
            {
                return (Forbid(), null, null);
            }
            if (!allowNurse)
            {
                this.ValidateUnitHeadOrInCharged(patient.UnitId, masterData, shiftService, redis);
            }
            await this.ValidateResourcePermissionAsync(patient);

            return (null, originalRecord, patient);
        }

        private async Task ValidateHemosheetForDoctorAsync(Guid hemoId)
        {
            if (this.IsDoctor())
            {
                HemodialysisRecord originalRecord = hemoService.GetHemodialysisRecord(hemoId) ?? throw new KeyNotFoundException();

                // Verify hemosheet's Patient
                Patient patient = FindPatient(originalRecord.PatientId) ?? throw new UnauthorizedException();
                await this.ValidateResourcePermissionAsync(patient);
            }
        }

        private void ValidateDoctorPatientAccess(Patient patient)
        {
            if (this.IsDoctorAndSeeOwnPatientOnly(setting.Value)
                && (!patient.DoctorId.HasValue || patient.DoctorId != User.GetUserIdAsGuid()))
            {
                throw new UnauthorizedException();
            }
        }
    }
}
