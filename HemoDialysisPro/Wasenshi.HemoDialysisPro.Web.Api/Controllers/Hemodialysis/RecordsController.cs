using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceStack.Messaging;
using ServiceStack.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Services.Core.Interfaces;
using Wasenshi.HemoDialysisPro.Services.Interfaces;
using Wasenshi.HemoDialysisPro.Share;
using Wasenshi.HemoDialysisPro.Models.TimezoneUtil;
using Wasenshi.HemoDialysisPro.ViewModels;
using Wasenshi.HemoDialysisPro.Web.Api.Controllers.Utils;
using Wasenshi.HemoDialysisPro.Utils;

namespace Wasenshi.HemoDialysisPro.Web.Api.Controllers
{
    [Route("api/HemoDialysis/[controller]", Order = -1)]
    [ApiController]
    public class RecordsController : PatientBaseController
    {
        private readonly IHemoService hemoService;
        private readonly IRecordService recordService;
        private readonly ICosignService cosignService;
        private readonly IShiftService shiftService;
        private readonly IMasterDataService masterData;
        private readonly IUserInfoService userInfo;
        private readonly IRedisClient redis;
        private readonly IMessageQueueClient message;
        private readonly IMapper mapper;

        public RecordsController(
            IHemoService hemoService,
            IRecordService recordService,
            ICosignService cosignService,
            IPatientService patientService,
            IVerifyPatientService verifyPatientService,
            IScheduleService scheduleService,
            IShiftService shiftService,
            IMasterDataService masterData,
            IUserInfoService userInfo,
            IRedisClient redis,
            IMessageQueueClient message,
            IMapper mapper) : base(patientService, verifyPatientService, scheduleService)
        {
            this.hemoService = hemoService;
            this.recordService = recordService;
            this.cosignService = cosignService;
            this.shiftService = shiftService;
            this.masterData = masterData;
            this.userInfo = userInfo;
            this.redis = redis;
            this.message = message;
            this.mapper = mapper;
        }

        [HttpGet("{hemoId}/dialysis")]
        public IActionResult GetAllDialysisRecordsForHemosheet(Guid hemoId)
        {
            //var hemosheet = hemoService.GetHemodialysisRecord(hemoId);

            //if (FindPatient(hemosheet?.PatientId) == null)
            //{
            //    return NotFound();
            //}

            IEnumerable<DialysisRecord> records = recordService.GetDialysisRecordsByHemoId(hemoId);

            IEnumerable<DialysisRecordViewModel> result =
                mapper.Map<IEnumerable<DialysisRecord>, IEnumerable<DialysisRecordViewModel>>(records);

            return Ok(result);
        }

        [HttpGet("{hemoId}/dialysis/update")]
        public IActionResult GetDialysisRecordsUpdateForHemosheet(Guid hemoId, DateTimeOffset? last_data, DateTimeOffset? last_machine_data)
        {
            IEnumerable<DialysisRecord> records = recordService.GetDialysisRecordsUpdateByHemoId(hemoId,
                                                                                                 last_data,
                                                                                                 last_machine_data);

            IEnumerable<DialysisRecordViewModel> result =
                mapper.Map<IEnumerable<DialysisRecord>, IEnumerable<DialysisRecordViewModel>>(records);

            return Ok(result);
        }

        [HttpGet("{hemoId}/dialysis/machine-update")]
        public IActionResult GetDialysisRecordsFromMachineForHemosheet(Guid hemoId, DateTimeOffset? last_data = null)
        {
            //var hemosheet = hemoService.GetHemodialysisRecord(hemoId);

            //if (FindPatient(hemosheet?.PatientId) == null)
            //{
            //    return NotFound();
            //}

            IEnumerable<DialysisRecord> records = recordService.GetMachineUpdateByHemoId(hemoId, last_data);

            IEnumerable<DialysisRecordViewModel> result =
                mapper.Map<IEnumerable<DialysisRecord>, IEnumerable<DialysisRecordViewModel>>(records);

            return Ok(result);
        }

        [HttpGet("dialysis/{id}")]
        public IActionResult GetDialysisRecord(Guid id)
        {
            var result = recordService.GetDialysisRecord(id);

            if (result == null)
            {
                return NotFound();
            }

            return Ok(mapper.Map<DialysisRecordViewModel>(result));
        }

        [HttpPost("dialysis")]
        public IActionResult CreateNewDialysisRecord([FromBody] DialysisRecordViewModel record, bool copyToNurse = false)
        {
            var newRecord = mapper.Map<DialysisRecord>(record);

            var dialysis = recordService.CreateDialysisRecord(newRecord);

            var result = new RecordResultViewModel
            {
                Dialysis = mapper.Map<DialysisRecordViewModel>(dialysis)
            };

            if (copyToNurse)
            {
                var nurse = recordService.CreateNurseRecord(new NurseRecord
                {
                    HemodialysisId = dialysis.HemodialysisId,
                    Content = dialysis.Note,
                    Timestamp = dialysis.Timestamp,
                    IsSystemUpdate = true
                });
                result.Nurse = mapper.Map<NurseRecordViewModel>(nurse);
            }

            // First record, auto update hemosheet start cycle
            if (recordService.GetDialysisRecordsByHemoId(dialysis.HemodialysisId).Count() <= 1)
            {
                var hemo = hemoService.GetHemodialysisRecord(dialysis.HemodialysisId);
                hemo.CycleStartTime = dialysis.Timestamp;
                hemoService.EditHemodialysisRecord(hemo);
            }

            return Ok(result);
        }

        [HttpPost("dialysis/{id}")]
        public IActionResult UpdateDialysisRecord(Guid id, [FromBody] DialysisRecordViewModel record)
        {
            record.Id = id;

            var original = recordService.GetDialysisRecord(id);

            var editRecord = mapper.Map(record, original);
            var result = recordService.UpdateDialysisRecord(editRecord);

            if (!result)
            {
                return NotFound();
            }

            return Ok(mapper.Map<DialysisRecordViewModel>(editRecord));
        }

        [HttpDelete("dialysis/{id}")]
        public IActionResult DeleteDialysisRecord(Guid id)
        {
            var result = recordService.DeleteDialysisRecord(id);
            if (!result)
            {
                return NotFound();
            }

            return Ok();
        }

        // ========================= Nurse ==========================

        [HttpGet("{hemoId}/nurse")]
        public IActionResult GetAllNurseRecordsForHemosheet(Guid hemoId)
        {
            IEnumerable<NurseRecord> records = recordService.GetNurseRecordsByHemoId(hemoId);

            IEnumerable<NurseRecordViewModel> result =
                mapper.Map<IEnumerable<NurseRecord>, IEnumerable<NurseRecordViewModel>>(records);

            return Ok(result);
        }

        [HttpGet("nurse/{id}")]
        public IActionResult GetNurseRecord(Guid id)
        {
            var result = recordService.GetNurseRecord(id);

            if (result == null)
            {
                return NotFound();
            }

            return Ok(mapper.Map<NurseRecordViewModel>(result));
        }

        [Authorize(Roles = Roles.NursesOnly)]
        [HttpPost("nurse")]
        public IActionResult CreateNewNurseRecord([FromBody] NurseRecordViewModel record)
        {
            var newRecord = mapper.Map<NurseRecord>(record);

            var result = recordService.CreateNurseRecord(newRecord);

            return Ok(mapper.Map<NurseRecordViewModel>(result));
        }

        [Authorize(Roles = Roles.NursesOnly)]
        [HttpPost("nurse/{id}")]
        public IActionResult UpdateNurseRecord(Guid id, [FromBody] NurseRecordViewModel record)
        {
            record.Id = id;

            var original = recordService.GetNurseRecord(id);

            var editRecord = mapper.Map(record, original);
            var result = recordService.UpdateNurseRecord(editRecord);

            if (!result)
            {
                return NotFound();
            }

            return Ok(mapper.Map<NurseRecordViewModel>(editRecord));
        }

        [Authorize(Roles = Roles.NursesOnly)]
        [HttpDelete("nurse/{id}")]
        public IActionResult DeleteNurseRecord(Guid id)
        {
            var result = recordService.DeleteNurseRecord(id);
            if (!result)
            {
                return NotFound();
            }

            return Ok();
        }

        // ========================= Doctor ==========================

        [HttpGet("{hemoId}/doctor")]
        public IActionResult GetAllDoctorRecordsForHemosheet(Guid hemoId)
        {
            IEnumerable<DoctorRecord> records = recordService.GetDoctorRecordsByHemoId(hemoId);

            IEnumerable<DoctorRecordViewModel> result =
                mapper.Map<IEnumerable<DoctorRecord>, IEnumerable<DoctorRecordViewModel>>(records);

            return Ok(result);
        }

        [HttpGet("doctor/{id}")]
        public IActionResult GetDoctorRecord(Guid id)
        {
            var result = recordService.GetDoctorRecord(id);

            if (result == null)
            {
                return NotFound();
            }

            return Ok(mapper.Map<DoctorRecordViewModel>(result));
        }

        [Authorize(Roles = Roles.DoctorUp)]
        [HttpPost("doctor")]
        public IActionResult CreateNewDoctorRecord([FromBody] DoctorRecordViewModel record)
        {
            var newRecord = mapper.Map<DoctorRecord>(record);

            var result = recordService.CreateDoctorRecord(newRecord);

            if (FeatureFlag.HasIntegrated())
            {
                var hemosheet = hemoService.GetHemodialysisRecord(record.HemodialysisId);
                var patient = PatientService.GetPatient(hemosheet.PatientId);
                var user = userInfo.FindUser(x => x.Id == User.GetUserIdAsGuid());
                string prefix;
                string name = "";
                if (User.IsInRole(Roles.Doctor) && !User.IsInRole(Roles.PowerAdmin))
                {
                    prefix = "{DoctorPrefix}";
                    name = user.User.FirstName + " " + user.User.LastName;
                }
                else
                {
                    prefix = "{AdminPrefix}";
                    name = user.User.FirstName ?? user.User.UserName;
                }

                var target = NotificationTarget.ForNurses(patient.UnitId);
                var noti = redis.AddNotification(
                    "DoctorNote_title",
                    $"DoctorNote_detail::{name}::{patient.Name}::{prefix}",
                    new[] { "modal", "hemosheet", hemosheet.Id.ToString(), "record" },
                    target,
                    "doctor-note"
                    );
                message.SendNotificationEvent(noti, target);
            }

            return Ok(mapper.Map<DoctorRecordViewModel>(result));
        }

        [Authorize(Roles = Roles.DoctorUp)]
        [HttpPost("doctor/{id}")]
        public IActionResult UpdateDoctorRecord(Guid id, [FromBody] DoctorRecordViewModel record)
        {
            record.Id = id;

            var original = recordService.GetDoctorRecord(id);

            var editRecord = mapper.Map(record, original);
            var result = recordService.UpdateDoctorRecord(editRecord);

            if (!result)
            {
                return NotFound();
            }

            return Ok(mapper.Map<DoctorRecordViewModel>(editRecord));
        }

        [Authorize(Roles = Roles.DoctorUp)]
        [HttpDelete("doctor/{id}")]
        public IActionResult DeleteDoctorRecord(Guid id)
        {
            var result = recordService.DeleteDoctorRecord(id);
            if (!result)
            {
                return NotFound();
            }

            return Ok();
        }

        // ========================== Execution / Medicine =========================================

        [HttpGet("{hemoId}/execution")]
        public IActionResult GetAllExecutionRecordsForHemosheet(Guid hemoId)
        {
            IEnumerable<ExecutionRecord> records = recordService.GetExecutionRecordsByHemoId(hemoId);

            IEnumerable<ExecutionRecordViewModel> result =
                mapper.Map<IEnumerable<ExecutionRecord>, IEnumerable<ExecutionRecordViewModel>>(records);

            return Ok(result);
        }

        [HttpGet("execution/{id}")]
        public IActionResult GetExecutionRecord(Guid id)
        {
            var result = recordService.GetExecutionRecord(id);

            if (result == null)
            {
                return NotFound();
            }

            return Ok(mapper.Map<ExecutionRecordViewModel>(result));
        }

        [Authorize(Roles = Roles.NursesOnly)]
        [HttpPost("{hemoId}/execution/medicine")]
        public IActionResult CreateNewMedicineRecords(Guid hemoId, string timezone, [FromBody] CreateMedicineRecordViewModel request)
        {
            TimeZoneInfo tz = TimeZoneInfo.Utc;
            if (!string.IsNullOrWhiteSpace(timezone))
            {
                tz = TimezoneUtils.GetTimeZone(timezone);
            }
            var result = recordService.CreateMedicineRecords(hemoId, request.Prescriptions, tz, false);

            if (!result.IsSuccess)
            {
                return BadRequest(result.Errors);
            }

            return Ok(mapper.Map<IEnumerable<ExecutionRecordViewModel>>(result.Records));
        }

        /// <summary>
        /// Generic API for any execution record.
        /// </summary>
        /// <param name="hemoId"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize(Roles = Roles.NursesOnly)]
        [HttpPost("{hemoId}/execution")]
        public IActionResult CreateNewExecutionRecord(Guid hemoId, [FromBody] CreateExecutionRecordViewModel request)
        {
            ExecutionRecord record;
            switch (request.Type)
            {
                case HemoDialysisPro.Models.Enums.ExecutionType.Medicine:
                    record = mapper.Map<MedicineRecord>(request);
                    break;
                case HemoDialysisPro.Models.Enums.ExecutionType.NSSFlush:
                    record = mapper.Map<FlushRecord>(request);
                    break;
                default:
                    throw new AppException("ExecutionTypeInvalid", "Unknown execution type. Please make sure to update your app to latest version. (If problem still exists, please contact administrator.)");
            }
            record.HemodialysisId = hemoId;
            var result = recordService.CreateExecutionRecord(record);

            return Ok(mapper.Map<ExecutionRecordViewModel>(result));
        }

        [Authorize(Roles = Roles.NotDoctorAndPN)]
        [HttpPost("execution/{id}")]
        public IActionResult UpdateExecutionRecord(Guid id, [FromBody] EditExecutionRecordViewModel body)
        {
            var record = recordService.GetExecutionRecord(id, body.Type);

            if (record == null)
            {
                return NotFound("The record does not exist.");
            }

            switch (body.Type)
            {
                case HemoDialysisPro.Models.Enums.ExecutionType.Medicine:
                    mapper.Map(body, (MedicineRecord)record);
                    break;
                case HemoDialysisPro.Models.Enums.ExecutionType.NSSFlush:
                    mapper.Map(body, (FlushRecord)record);
                    break;
                default:
                    mapper.Map(body, record);
                    break;
            }

            var result = recordService.UpdateExecutionRecord(record);

            return Ok(mapper.Map<ExecutionRecordViewModel>(record));
        }

        [Authorize(Roles = Roles.NotDoctorAndPN)]
        [HttpPost("execution/{id}/execute")]
        public IActionResult ExecuteRecord(Guid id, [FromBody] ExecuteViewModel body)
        {
            var record = recordService.GetExecutionRecord(id, body.Type);

            if (record == null)
            {
                return NotFound("The record does not exist.");
            }

            record.IsExecuted = true;
            if (body.Timestamp.HasValue)
            {
                record.Timestamp = body.Timestamp.GetValueOrDefault().UtcDateTime;
            }

            var result = recordService.UpdateExecutionRecord(record);

            return Ok(mapper.Map<ExecutionRecordViewModel>(record));
        }

        [Authorize(Roles = Roles.NursesOnly)]
        [HttpPost("execution/{id}/cosign")]
        public async Task<IActionResult> AddCosignAsync(Guid id, [FromBody] CosignRequestViewModel cosign)
        {
            if (string.IsNullOrWhiteSpace(cosign.Password))
            {
                //TODO: add notification system and implement approval request feature
                return ValidationProblem("Password cannot be null");
            }

            bool result = await cosignService.AssignCosignForExecutionRecord(id, cosign.UserId, cosign.Password);
            if (!result)
            {
                return Forbid();
            }

            return Ok();
        }

        [Authorize(Roles = Roles.NotDoctorAndPN)]
        [HttpPut("execution/{id}/claim")]
        public async Task<IActionResult> ClaimExecution(Guid id)
        {
            var result = recordService.ClaimExecutionRecord(id, User.GetUserIdAsGuid());
            if (!result)
            {
                return BadRequest();
            }

            return Ok();
        }

        [Authorize(Roles = Roles.HeadNurseOnly)]
        [HttpDelete("execution/{id}")]
        public IActionResult DeleteExecutionRecord(Guid id)
        {
            var record = recordService.GetExecutionRecord(id);
            if (record == null)
            {
                return NotFound();
            }
            var unitId = PatientService.GetPatient(record.Hemodialysis.PatientId).UnitId;
            this.ValidateUnitHeadOrInCharged(unitId, masterData, shiftService, redis);

            var result = recordService.DeleteExecutionRecord(id);
            if (!result)
            {
                return NotFound();
            }

            return Ok();
        }
    }
}
