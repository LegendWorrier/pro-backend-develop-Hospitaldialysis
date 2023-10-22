using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ServiceStack.Messaging;
using ServiceStack.Redis;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Wasenshi.AuthPolicy;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Models.Enums;
using Wasenshi.HemoDialysisPro.Services.Core.Interfaces;
using Wasenshi.HemoDialysisPro.Services.Interfaces;
using Wasenshi.HemoDialysisPro.Share;
using Wasenshi.HemoDialysisPro.Models.TimezoneUtil;
using Wasenshi.HemoDialysisPro.ViewModels;
using Wasenshi.HemoDialysisPro.Web.Api.ApproveRequest;
using Wasenshi.HemoDialysisPro.Utils;

namespace Wasenshi.HemoDialysisPro.Web.Api.Controllers
{
    [Authorize(Policy = Feature.INTEGRATE)]
    [Route("api/[controller]")]
    [ApiController]
    public class RequestController : ControllerBase
    {
        private readonly ILogger<RequestController> logger;
        private readonly IConfiguration config;
        private readonly IRedisClient redis;
        private readonly IMessageQueueClient message;
        private readonly IMasterDataService master;
        private readonly IPatientService patientService;
        private readonly IUserInfoService userInfo;
        private readonly IHemoService hemoService;
        private readonly IRecordService recordService;
        private readonly IScheduleService scheduleService;
        private readonly IShiftService shiftService;
        private readonly ICosignService cosignService;

        public RequestController(
            ILogger<RequestController> logger,
            IConfiguration config,
            IRedisClient redis,
            IMessageQueueClient message,
            IMasterDataService master,
            IPatientService patientService,
            IUserInfoService userInfo,
            IHemoService hemoService,
            IRecordService recordService,
            IScheduleService scheduleService,
            IShiftService shiftService,
            ICosignService cosignService
            )
        {
            this.logger = logger;
            this.config = config;
            this.redis = redis;
            this.message = message;
            this.master = master;
            this.patientService = patientService;
            this.userInfo = userInfo;
            this.hemoService = hemoService;
            this.recordService = recordService;
            this.scheduleService = scheduleService;
            this.shiftService = shiftService;
            this.cosignService = cosignService;
        }

        [HttpPost("cosign/hemosheet/{hemoId}")]
        public IActionResult CosignHemosheet(Guid hemoId, CosignRequestViewModel request)
        {
            var requestInfo = new CosignHemoRequest
            {
                Requester = User.GetUserIdAsGuid(),
                Approver = request.UserId,
                HemoId = hemoId
            };

            var user = userInfo.FindUser(x => x.Id == request.UserId);
            if (user == null) return NotFound();

            var hemo = hemoService.GetHemodialysisRecord(hemoId);
            var patient = patientService.GetPatient(hemo.PatientId);

            var key = GetResourceKey(requestInfo);
            var keyNameArg = $"{{{key}}}";
            var noti = NotifyApprover(requestInfo,
                new[] { keyNameArg },
                new[] { keyNameArg, patient.Name },
                key
                );

            var saved = SaveRequest(requestInfo, noti.Id);
            if (saved == null)
            {
                return StatusCode(500, "Something went wrong. Sorry for inconvinence.");
            }

            return Ok();
        }

        [HttpPost("cosign/execution/{id}")]
        public IActionResult CosignExecution(Guid id, CosignRequestViewModel request)
        {
            var requestInfo = new CosignExeRequest
            {
                Requester = User.GetUserIdAsGuid(),
                Approver = request.UserId,
                ExecutionId = id
            };

            var user = userInfo.FindUser(x => x.Id == request.UserId);
            if (user == null) return NotFound();

            var record = recordService.GetExecutionRecord(id);
            var hemo = hemoService.GetHemodialysisRecord(record.HemodialysisId);
            var patient = patientService.GetPatient(hemo.PatientId);

            var key = GetResourceKey(requestInfo);
            var keyNameArg = $"{{{key}}}";
            var noti = NotifyApprover(requestInfo,
                new[] { keyNameArg },
                new[] { keyNameArg, patient.Name },
                key
                );

            var saved = SaveRequest(requestInfo, noti.Id);
            if (saved == null)
            {
                return StatusCode(500, "Something went wrong. Sorry for inconvinence.");
            }

            return Ok();
        }

        [HttpPost("prescription-nurse/{id}")]
        public IActionResult PrescriptionNurse(Guid id, CosignRequestViewModel request)
        {
            var requestInfo = new PrescriptionNurseRequest
            {
                Requester = User.GetUserIdAsGuid(),
                Approver = request.UserId,
                PrescriptionId = id
            };

            var user = userInfo.FindUser(x => x.Id == request.UserId);
            if (user == null) return NotFound();
            if (user.Roles.Contains(Roles.Doctor))
            {
                return BadRequest();
            }

            var prescription = hemoService.GetDialysisPrescription(id);
            var patient = patientService.GetPatient(prescription.PatientId);

            var key = GetResourceKey(requestInfo);
            var keyNameArg = $"{{{key}}}";
            var noti = NotifyApprover(requestInfo,
                new[] { keyNameArg },
                new[] { patient.Name },
                key
                );

            var saved = SaveRequest(requestInfo, noti.Id);
            if (saved == null)
            {
                return StatusCode(500, "Something went wrong. Sorry for inconvinence.");
            }

            return Ok();
        }

        [Authorize(Roles = Roles.NotPN)]
        [HttpPost("transfer/{sectionId}/{slot}/{patientId}")]
        public IActionResult TransferRequest(string patientId, int sectionId, SectionSlots slot, [FromBody] TransferRequest.TransferTarget targetSlot)
        {
            var patient = patientService.GetPatient(patientId);
            if (targetSlot.UnitId == 0)
            {
                return BadRequest("The target unit must be specified.");
            }
            if (patient.UnitId == targetSlot.UnitId)
            {
                return BadRequest("The target unit cannot be the same with patient's unit.");
            }

            var requestInfo = new TransferRequest
            {
                Target = targetSlot,
                PatientId = patientId,
                SectionId = sectionId,
                Slot = slot,
                Requester = User.GetUserIdAsGuid(),
                TargetUnitId = targetSlot.UnitId,
                ExtraNotifyRole = Roles.Nurse,
                ExtraNotifyUnitId = patient.UnitId
            };

            var units = redis.GetMonitorPool().UnitListFromCache();
            var originalUnit = units.FirstOrDefault(x => x.Id == patient.UnitId);
            var targetUnit = units.FirstOrDefault(x => x.Id == targetSlot.UnitId);
            if (originalUnit == null || targetUnit == null)
            {
                logger.LogWarning("Trying to refer to non-existing UnitId. [target: {TargetId}, original: {OriginalId}]", targetSlot.UnitId, patient.UnitId);
                return StatusCode(500, "Something went wrong. Sorry for inconvinence.");
            }

            var section = scheduleService.GetSection(targetSlot.SectionId);
            var date = ConvertStartTimeToDate(section.StartTime, targetSlot.Slot);
            var key = GetResourceKey(requestInfo);
            var keyNameArg = $"{{{key}}}";
            var noti = NotifyApprover(requestInfo,
                new[] { keyNameArg },
                new[] { keyNameArg, patient.Name, originalUnit.Name, targetUnit.Name, ConvertToArg(date) },
                "Transfer",
                NotificationTarget.ForNurses(targetSlot.UnitId)
                );

            var saved = SaveRequest(requestInfo, noti.Id);
            if (saved == null)
            {
                return StatusCode(500, "Something went wrong. Sorry for inconvinence.");
            }

            // Extra notify
            NotifyExtra(requestInfo,
                new[] { keyNameArg },
                new[] { keyNameArg, patient.Name, targetUnit.Name, ConvertToArg(date) },
                new[] { "page", "schedule" },
                "Transfer"
                );

            return Ok();
        }

        [Authorize(Roles = Roles.NotPN)]
        [HttpPost("transfer/{sectionId}/{slot}/{patientId}/temp")]
        public IActionResult TransferRequestTemp(string patientId, int sectionId, SectionSlots slot, [FromBody] RescheduleViewModel reschedule)
        {
            if (!reschedule.OverrideUnitId.HasValue)
            {
                return BadRequest("override unit must have value");
            }
            var patient = patientService.GetPatient(patientId);
            if (patient.UnitId == reschedule.OverrideUnitId)
            {
                return BadRequest("Override unit cannot be the same with patient's unit.");
            }

            if (!string.IsNullOrWhiteSpace(reschedule.TargetPatientId) && !reschedule.OriginalDate.HasValue)
            {
                return BadRequest("OriginalDate cannot be null when TargetPatientId is specified.");
            }

            scheduleService.VerifySchedule(new Schedule
            {
                PatientId = patientId,
                SectionId = sectionId,
                Slot = slot,
                Date = reschedule.Date.UtcDateTime,
                OverrideUnitId = reschedule.OverrideUnitId,
                OriginalDate = reschedule.OriginalDate?.UtcDateTime
            });

            var requestInfo = new TempTransferRequest
            {
                Request = reschedule,
                PatientId = patientId,
                SectionId = sectionId,
                Slot = slot,
                Requester = User.GetUserIdAsGuid(),
                TargetUnitId = reschedule.OverrideUnitId,
                ExtraNotifyRole = Roles.Nurse,
                ExtraNotifyUnitId = patient.UnitId
            };

            var units = redis.GetMonitorPool().UnitListFromCache();
            var originalUnit = units.FirstOrDefault(x => x.Id == patient.UnitId);
            var targetUnit = units.FirstOrDefault(x => x.Id == reschedule.OverrideUnitId);
            if (originalUnit == null || targetUnit == null)
            {
                logger.LogWarning("Trying to refer to non-existing UnitId. [target: {TargetId}, original: {OriginalId}]", reschedule.OverrideUnitId, patient.UnitId);
                return StatusCode(500, "Something went wrong. Sorry for inconvinence.");
            }

            var key = GetResourceKey(requestInfo);
            var keyNameArg = $"{{{key}}}";
            var noti = NotifyApprover(requestInfo,
                new[] { keyNameArg },
                new[] { patient.Name, originalUnit.Name, targetUnit.Name, ConvertToArg(reschedule.Date), keyNameArg },
                "Transfer",
                NotificationTarget.ForNurses(reschedule.OverrideUnitId.Value)
                );

            var saved = SaveRequest(requestInfo, noti.Id);
            if (saved == null)
            {
                return StatusCode(500, "Something went wrong. Sorry for inconvinence.");
            }

            // Extra notify
            NotifyExtra(requestInfo,
                new[] { keyNameArg },
                new[] { patient.Name, targetUnit.Name, ConvertToArg(reschedule.Date), keyNameArg },
                new[] { "page", "schedule" },
                "Transfer"
                );

            return Ok();
        }

        [HttpGet("{id}")]
        public IActionResult RequestInfo(Guid id)
        {
            var requestRepo = redis.As<RequestApprove>();
            var request = requestRepo.GetById(id);

            return Ok(request);
        }

        [HttpPost("{id}/approve")]
        public IActionResult Approve(Guid id, bool deny = false)
        {
            var requestRepo = redis.As<RequestApprove>();
            var request = requestRepo.GetById(id);
            if (request == null)
            {
                return BadRequest("Request not found or has expired.");
            }

            if (request.Approver.HasValue && request.Approver.Value != User.GetUserIdAsGuid())
            {
                return Unauthorized();
            }
            if (!User.IsInRole(Roles.PowerAdmin) && request.TargetUnitId.HasValue && !User.GetUnitList().Contains(request.TargetUnitId.Value))
            {
                return Unauthorized();
            }

            Exception error = null;
            string result = deny ? "denied" : "approved";
            try
            {
                VerifyUserAndPermission(request);

                if (!deny)
                {
                    ProcessRequest(request);
                }

                requestRepo.DeleteById(request.Id);
                ProcessApproveNotify(request, deny);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to process request [type: {RequestType}]", request.Type);
                error = e;
                throw;
            }
            finally
            {
                if (error is not UnauthorizedException)
                {
                    requestRepo.DeleteById(request.Id);
                    if (error != null)
                    {
                        redis.SetRequestNotiInvalid(request.NotificationId);
                    }
                }
            }

            return Ok(result);
        }

        private void ProcessRequest(RequestApprove request)
        {
            switch (request.Type)
            {
                case ApproveRequest.TransferRequest.KEY:
                    {
                        var transfer = request as TransferRequest;
                        Transfer(transfer);
                        break;
                    }
                case TempTransferRequest.KEY:
                    {
                        var transfer = request as TempTransferRequest;
                        TransferTemp(transfer);
                        break;
                    }
                case CosignHemoRequest.KEY:
                    {
                        var cosign = request as CosignHemoRequest;
                        CosignHemo(cosign);
                        break;
                    }
                case CosignExeRequest.KEY:
                    {
                        var cosign = request as CosignExeRequest;
                        CosignExecution(cosign);
                        break;
                    }
                case PrescriptionNurseRequest.KEY:
                    {
                        var nurse = request as PrescriptionNurseRequest;
                        PrescriptionNurse(nurse);
                        break;
                    }

                default:
                    logger.LogInformation("Request type unknown. Skip processing.");
                    break;
            }
        }

        // ================================ Util ================================

        /// <summary>
        /// Used to notify the target approver(s) and allow them to response with approve or deny.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="titleArgs"></param>
        /// <param name="detailArgs"></param>
        /// <param name="tag"></param>
        /// <param name="extraTarget"></param>
        /// <returns></returns>
        private Notification NotifyApprover(RequestApprove request, string[] titleArgs, string[] detailArgs, string tag, NotificationTarget extraTarget = null)
        {
            var key = GetResourceKey(request);
            var target = GetApproverTarget(request, extraTarget);
            var noti = redis.AddNotification(
                $"{key}_title" + (titleArgs?.Length > 0 ? $"::{string.Join("::", titleArgs)}" : ""),
                $"{key}_detail" + (detailArgs?.Length > 0 ? $"::{string.Join("::", detailArgs)}" : ""),
                new[] { "approve", request.Id.ToString() },
                NotificationType.ActionRequired,
                target,
                DateTime.UtcNow.AddHours(18),
                tag
                );
            message.SendNotificationEvent(noti, target);

            return noti;
        }

        /// <summary>
        /// Used to notify also, to the requester (and any extra target specified in request) to inform what has been requested.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="titleArgs"></param>
        /// <param name="detailArgs"></param>
        /// <param name="action"></param>
        /// <param name="tag"></param>
        private void NotifyExtra(RequestApprove request, string[] titleArgs, string[] detailArgs, string[] action, string tag)
        {
            var key = GetResourceKey(request);
            var target = GetRequesterTarget(request);
            var noti = redis.AddNotification(
                $"{key}Info_title" + (titleArgs?.Length > 0 ? $"::{string.Join("::", titleArgs)}" : ""),
                $"{key}Info_detail" + (detailArgs?.Length > 0 ? $"::{string.Join("::", detailArgs)}" : ""),
                action ?? Array.Empty<string>(),
                target,
                tag, "info"
                );
            message.SendNotificationEvent(noti, target);
        }

        private static NotificationTarget GetApproverTarget(RequestApprove request, NotificationTarget extraTarget = null)
        {
            if (extraTarget == null && !request.Approver.HasValue)
            {
                throw new InvalidOperationException("Invalid! this request has no target approver(s) to notify to.");
            }

            if (extraTarget != null && request.Approver.HasValue)
            {
                return extraTarget.WithUser(request.Approver.Value);
            }

            return extraTarget ?? NotificationTarget.ForUser(request.Approver.Value);
        }

        private NotificationTarget GetRequesterTarget(RequestApprove request)
        {
            var requester = userInfo.FindUser(x => x.Id == request.Requester);
            bool alreadyHasRequester = false;

            NotificationTarget target = null;
            // extra target
            if (request.ExtraNotifyUnitId.HasValue)
            {
                if (!string.IsNullOrWhiteSpace(request.ExtraNotifyRole))
                {
                    target = request.ExtraNotifyRole switch
                    {
                        Roles.Nurse => NotificationTarget.ForNurses(request.ExtraNotifyUnitId.Value),
                        Roles.HeadNurse => NotificationTarget.ForHeadNurses(request.ExtraNotifyUnitId.Value),
                        Roles.Doctor => NotificationTarget.ForDoctors(request.ExtraNotifyUnitId.Value),
                        _ => throw new InvalidOperationException($"Unknown target role for notification. [{request.ExtraNotifyRole}]"),
                    };
                    var targetRoles = target.Roles;
                    if (targetRoles.Any(x => requester.Roles.Contains(x)) && requester.User.Units.Any(x => x.UnitId == request.ExtraNotifyUnitId.Value))
                    {
                        alreadyHasRequester = true;
                    }
                }
                else
                {
                    target = NotificationTarget.ForUnit(request.ExtraNotifyUnitId.Value);
                    if (requester.User.Units.Any(x => x.UnitId == request.ExtraNotifyUnitId.Value))
                    {
                        alreadyHasRequester = true;
                    }
                }

                if (!alreadyHasRequester)
                {
                    target.WithUser(requester.User.Id);
                }
            }
            else
            {
                target = NotificationTarget.ForUser(request.Requester);
            }

            return target;
        }

        private RequestApprove SaveRequest(RequestApprove requestInfo, Guid notificationId)
        {
            requestInfo.NotificationId = notificationId;
            var requestRepo = redis.As<RequestApprove>();
            var saved = requestRepo.Store(requestInfo, TimeSpan.FromHours(18)); // expire in 18 hours
            return saved;
        }

        /// <summary>
        /// Notify back to the requester (and any extra target specified in request) for the result of a request (approved or rejected).
        /// </summary>
        /// <param name="request"></param>
        /// <param name="deny"></param>
        /// <exception cref="AppException"></exception>
        private void ProcessApproveNotify(RequestApprove request, bool deny)
        {
            var originalNoti = redis.GetNotification(request.NotificationId) ?? throw new AppException("NULL", "Cannot find the associated notification.");
            if (originalNoti.Tags.Contains("approved") || originalNoti.Tags.Contains("denied"))
            {
                throw new AppException("REPLIED", "The request has already been replied with either approved or denied.");
            }

            NotificationTarget target = GetRequesterTarget(request);

            var noti = redis.AddNotification(
                GetApproveResourceKey(request, "title", deny),
                GetApproveResourceKey(request, "detail", deny),
                Array.Empty<string>(),
                target,
                deny ? "denied" : "approved"
                );
            message.SendNotificationEvent(noti, target);

            if (deny)
            {
                redis.DenyNotification(originalNoti);
            }
            else
            {
                redis.ApproveNotification(originalNoti);
            }
        }

        private static string GetApproveResourceKey(RequestApprove request, string subfix, bool deny)
        {
            string prefix = GetResourceKey(request);
            string type = deny ? "Denied" : "Approved";
            string resourceKey = HasCustomApproveResource(request)
                ? $"{prefix}{type}_{subfix}"
                : $"{type}_{subfix}::{{{prefix}}}";
            return resourceKey;
        }

        private static string GetResourceKey(RequestApprove request)
        {
            return request.Type switch
            {
                TempTransferRequest.KEY => "TransferTemp",
                ApproveRequest.TransferRequest.KEY => "Transfer",
                CosignHemoRequest.KEY => "CosignHemo",
                CosignExeRequest.KEY => "CosignExe",
                PrescriptionNurseRequest.KEY => "PrescNurse",
                _ => "",
            };
        }
        private static bool HasCustomApproveResource(RequestApprove request)
        {
            return request.Type switch
            {
                //"transfer" => true,
                _ => false,
            };
        }

        private static string ConvertToArg(DateTimeOffset date)
        {
            return date.ToUniversalTime().ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss+00:00", CultureInfo.InvariantCulture);
        }

        private DateTimeOffset ConvertStartTimeToDate(TimeOnly startTime, SectionSlots targetDay)
        {
            var tz = TimezoneUtils.GetTimeZone(config["TIMEZONE"]);
            var result = new DateTimeOffset(1992, 1, 1, startTime.Hour, startTime.Minute, 0, tz.BaseUtcOffset);
            var startDay = result.DayOfWeek;
            var targetDayWeek = targetDay == SectionSlots.Sun ? DayOfWeek.Sunday : (DayOfWeek)(targetDay + 1);
            return result.AddDays(targetDayWeek - startDay);
        }

        // ================================ process requests ==========================================
        #region Process Requests

        private void TransferTemp(TempTransferRequest transfer)
        {
            var patient = patientService.GetPatient(transfer.PatientId);
            List<Schedule> schedules = new()
            {
                new Schedule
                {
                    PatientId = transfer.PatientId,
                    SectionId = transfer.SectionId,
                    Slot = transfer.Slot,
                    Date = transfer.Request.Date.UtcDateTime,
                    OverrideUnitId = transfer.Request.OverrideUnitId,
                    OriginalDate = transfer.Request.OriginalDate?.UtcDateTime
                }
            };

            if (transfer.Request.TargetPatientId != null)
            {
                schedules.Add(new Schedule
                {
                    PatientId = transfer.Request.TargetPatientId,
                    Date = transfer.Request.OriginalDate.Value.UtcDateTime,
                    OverrideUnitId = transfer.Request.OverrideUnitId.HasValue ? patient.UnitId : (int?)null,
                    OriginalDate = transfer.Request.Date.UtcDateTime,
                    Patient = new Patient { UnitId = transfer.Request.OverrideUnitId ?? patient.UnitId }
                });
            }

            scheduleService.Reschedule(schedules);
        }

        private void Transfer(TransferRequest transfer)
        {
            var first = new SectionSlotPatient
            {
                PatientId = transfer.PatientId,
                SectionId = transfer.SectionId,
                Slot = transfer.Slot
            };
            var second = new SectionSlotPatient
            {
                SectionId = transfer.Target.SectionId,
                Slot = transfer.Target.Slot
            };

            bool result = scheduleService.SwapSlot(first, second);
            if (result)
            {
                var patient = patientService.GetPatient(transfer.PatientId);
                patient.UnitId = transfer.TargetUnitId.Value;
                result = patientService.UpdatePatient(patient);
                if (!result)
                {
                    logger.LogWarning("Failed to change unit for patient while processing transfer request [patient: {PatientId}, {PatientName}]", patient.Id, patient.Name);
                }
            }
        }

        private void CosignHemo(CosignHemoRequest cosign)
        {
            bool result = cosignService.AssignCosignForHemosheet(cosign.HemoId, cosign.Approver.Value).ConfigureAwait(false).GetAwaiter().GetResult();
            if (!result)
            {
                throw new AppException($"Save cosign failed. [hemoId: {cosign.HemoId}, approver: {cosign.Approver}]");
            }
        }

        private void CosignExecution(CosignExeRequest cosign)
        {
            bool result = cosignService.AssignCosignForExecutionRecord(cosign.ExecutionId, cosign.Approver.Value).ConfigureAwait(false).GetAwaiter().GetResult();
            if (!result)
            {
                throw new AppException($"Save cosign failed. [executionId: {cosign.ExecutionId}, approver: {cosign.Approver}]");
            }
        }

        private void PrescriptionNurse(PrescriptionNurseRequest nurse)
        {
            bool result = cosignService.AssignNurseForDialysisPrescription(nurse.PrescriptionId, nurse.Approver.Value).ConfigureAwait(false).GetAwaiter().GetResult();
            if (!result)
            {
                throw new AppException($"Save prescription nurse failed. [prescriptionId: {nurse.PrescriptionId}, approver: {nurse.Approver}]");
            }
        }

        #endregion

        #region Verify User
        private void VerifyUserAndPermission(RequestApprove request)
        {
            switch (request.Type)
            {
                case TempTransferRequest.KEY:
                case ApproveRequest.TransferRequest.KEY:
                    VerifyHeadNurseOrInchargePermission(request.TargetUnitId.Value);
                    break;

                default:
                    VerifyIsApprover(request);
                    break;
            }
        }

        private void VerifyIsApprover(RequestApprove requestInfo)
        {
            if (User.GetUserIdAsGuid() != requestInfo.Approver.Value)
            {
                throw new UnauthorizedException();
            }
        }

        private void VerifyHeadNurseOrInchargePermission(int targetUnitId)
        {
            if (!master.IsUnitHead(User.GetUserIdAsGuid()) && !User.IsInRole(Roles.HeadNurse))
            {
                bool incharge = shiftService.IsIncharge(User.GetUserIdAsGuid(), targetUnitId, redis.GetUnitShift(targetUnitId)?.CurrentSection);
                if (!incharge)
                {
                    throw new UnauthorizedException();
                }
            }
        }
        #endregion

    }
}
