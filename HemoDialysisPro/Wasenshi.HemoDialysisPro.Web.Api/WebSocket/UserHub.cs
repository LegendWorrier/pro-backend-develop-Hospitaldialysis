using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;
using ServiceStack.Redis;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Services.Core.Interfaces;
using Wasenshi.HemoDialysisPro.Services.Interfaces;
using Wasenshi.HemoDialysisPro.Share;
using Wasenshi.HemoDialysisPro.Models.TimezoneUtil;
using Wasenshi.HemoDialysisPro.ViewModels;
using Wasenshi.HemoDialysisPro.Web.Api.Utils;
using Wasenshi.HemoDialysisPro.Utils;

namespace Wasenshi.HemoDialysisPro.Web.Api.WebSocket
{
    [Authorize]
    public class UserHub : Hub<IUserClient>
    {
        private readonly MonitorPool monitorPool;
        private readonly IHubContext<HemoBoxHub, IHemoBoxClient> hemobox;
        private readonly IPatientService patientService;
        private readonly IHemoService hemoService;
        private readonly IRecordService recordService;
        private readonly IUserInfoService userInfoService;
        private readonly IRedisPool redisPool;
        private readonly IHtmlLocalizer<Notification> localizer;
        private readonly ILogger<UserHub> logger;
        private readonly IMapper mapper;

        private readonly TimeZoneInfo tz;

        // channel
        public const string NO_UNIT_CHANNEL = "no-unit";
        public const string UNIT_CHANNEL_PREFIX = "unit-";
        public const string ROLE_CHANNEL_PREFIX = "role-";
        public const string ROOT_ADMIN = "root-admin";
        public static string GetUnitChannelName(int unitId) => $"{UNIT_CHANNEL_PREFIX}{unitId}";
        public static string GetRoleChannel(string role) => $"{ROLE_CHANNEL_PREFIX}{role}";
        public static string GetUnitRoleChannel(int unitId, string role) => $"{UNIT_CHANNEL_PREFIX}{unitId}-{ROLE_CHANNEL_PREFIX}{role}";

        public UserHub(
            IRedisClient redis,
            IHubContext<HemoBoxHub, IHemoBoxClient> hemobox,
            IPatientService patientService,
            IHemoService hemoService,
            IRecordService recordService,
            IUserInfoService userInfoService,
            IRedisPool redisPool,
            IHtmlLocalizer<Notification> localizer,
            ILogger<UserHub> logger,
            IMapper mapper,
            IConfiguration config
            )
        {
            this.monitorPool = redis.GetMonitorPool();
            this.hemobox = hemobox;
            this.patientService = patientService;
            this.hemoService = hemoService;
            this.recordService = recordService;
            this.userInfoService = userInfoService;
            this.redisPool = redisPool;
            this.localizer = localizer;
            this.logger = logger;
            this.mapper = mapper;

            tz = TimezoneUtils.GetTimeZone(config["TIMEZONE"]);
        }

        public async Task<bool> ChangeBoxUnit(string macAddress, int unitId)
        {
            var bed = monitorPool.GetBedByMacAddress(macAddress);
            if (bed == null)
            {
                Log.Warning($"User attemp to change name for a non-existing hemobox macaddress [{macAddress}]");
                return false;
            }
            if (!CheckUnitPermission(bed))
            {
                Log.Warning($"User attempt to change box's unit but is not root admin [{macAddress}, unit: {bed.UnitId}]");
                return false;
            }

            if (bed.UnitId.HasValue)
            {
                await Clients.Group(GetUnitChannelName(bed.UnitId.Value)).BoxChangeUnit(bed.MacAddress, unitId);
            }

            bed.UnitId = unitId;

            var connectionId = bed.ConnectionId;
            if (!string.IsNullOrEmpty(connectionId))
            {
                await hemobox.Clients.Client(connectionId).UnitChanged(unitId);
            }
            // To ensure the box really change their state successfully, so we let the box do update event job.

            return true;
        }

        public async Task<bool> ChangeBedName(string macAddress, string newName)
        {
            var bed = monitorPool.GetBedByMacAddress(macAddress);
            if (bed == null)
            {
                Log.Warning($"User attemp to change name for a non-existing hemobox macaddress [{macAddress}]");
                return false;
            }
            if (!CheckUnitPermission(bed))
            {
                Log.Warning($"User attempt to change a box state without authorized permission [{macAddress}, unit: {bed.UnitId}]");
                return false;
            }
            bed.Name = newName;
            // TODO: save to DB for registered hemobox
            var connectionId = bed.ConnectionId;
            if (!string.IsNullOrEmpty(connectionId))
            {
                await hemobox.Clients.Client(connectionId).NameChanged(newName);
            }
            // To ensure the box really change their state successfully, so we let the box do update event job.

            return true;
        }

        public async Task<bool> ChangeBoxState(string macAddress)
        {
            var bed = monitorPool.GetBedByMacAddress(macAddress);
            if (bed == null)
            {
                Log.Warning($"User attemp to change state for a non-existing hemobox macaddress [{macAddress}]");
                return false;
            }
            if (!CheckUnitPermission(bed))
            {
                Log.Warning($"User attempt to change a box state without authorized permission [{macAddress}, unit: {bed.UnitId}]");
                return false;
            }
            if (bed.PatientId == null)
            {
                return false;
            }

            var connectionId = bed.ConnectionId;
            if (!string.IsNullOrEmpty(connectionId))
            {
                await hemobox.Clients.Client(connectionId).ChangeState();
            }
            // To ensure the box really change their state successfully, so we let the box do update event job.

            return true;
        }

        public async Task<bool> Complete(string macAddress)
        {
            var bed = monitorPool.GetBedByMacAddress(macAddress);
            if (bed == null)
            {
                Log.Warning($"User attemp to change state for a non-existing hemobox macaddress [{macAddress}]");
                return false;
            }
            if (!CheckUnitPermission(bed))
            {
                Log.Warning($"User attempt to change a box state without authorized permission [{macAddress}, unit: {bed.UnitId}]");
                return false;
            }
            if (bed.PatientId == null)
            {
                return false;
            }

            var connectionId = bed.ConnectionId;
            if (!string.IsNullOrEmpty(connectionId))
            {
                await hemobox.Clients.Client(connectionId).Complete();
            }
            // To ensure the box really change their state successfully, so we let the box do update event job.

            return true;
        }

        public async Task<PatientInfo> PickPatient(string macAddress, string patientId)
        {
            if (patientId == null)
            {
                return null;
            }
            var bed = monitorPool.GetBedByMacAddress(macAddress);
            if (bed == null)
            {
                Log.Warning($"User attempt to change state for a non-existing hemobox macaddress [{macAddress}]");
                return null;
            }
            if (!CheckUnitPermission(bed))
            {
                Log.Warning($"User attempt to change a box state without authorized permission [{macAddress}, unit: {bed.UnitId}]");
                return null;
            }
            var patient = patientService.GetPatient(patientId);
            if (patient == null)
            {
                return null;
            }

            float weight = 0;
            var hemosheet = hemoService.GetHemodialysisRecordByPatientId(patientId);
            if (hemosheet != null)
            {
                weight = hemosheet.Dehydration.PreTotalWeight;
            }
            var result = new PatientInfo
            {
                Id = patient.Id,
                Name = patient.Name,
                Rfid = patient.RFID ?? "",
                UnitId = patient.UnitId, // TODO: check schedule and replace with current schedule's unit
                Weight = weight
            };
            bed.Patient = result;
            bed.PatientId = patientId;
            monitorPool.AddOrUpdateBed(bed);
            var connectionId = bed.ConnectionId;
            if (!string.IsNullOrEmpty(connectionId))
            {
                await hemobox.Clients.Client(connectionId).PatientSelect(result);
            }

            await HemoBoxHubHelpers.DispatchHemoBoxUpdateEvent(bed, Clients, mapper, t => t.BedPatient(macAddress, result));
            return result;
        }

        public async Task<DialysisRecordViewModel> GetData(DataRequest request)
        {
            BedBoxInfo bed = null;
            if (!string.IsNullOrEmpty(request.MacAddress))
            {
                bed = monitorPool.GetBedByMacAddress(request.MacAddress);
            }
            else if (!string.IsNullOrEmpty(request.PatientId))
            {
                bed = monitorPool.GetBedByPatientId(request.PatientId);
            }

            if (bed == null || string.IsNullOrEmpty(bed.ConnectionId)) // target not found
            {
                return null;
            }

            (var record, var hemosheet) = await HemoBoxHub.GetDataWithResponseAsync(new GetDataRequiredServices
            {
                HubContext = hemobox,
                Monitor = monitorPool,
                HemoService = hemoService,
                RecordService = recordService,
                Mapper = mapper
            }, bed.ConnectionId);

            if (record == null || hemosheet == null)
            {
                return null;
            }

            // auto copy to current hemosheet immediately

            var result = mapper.Map<DialysisRecord>(record);
            result.Id = Guid.Empty;
            result.HemodialysisId = hemosheet.Id;
            result.IsFromMachine = false;
            result.IsSystemUpdate = false;

            recordService.CreateDialysisRecord(result);

            return mapper.Map<DialysisRecordViewModel>(result);
        }

        public Task<IEnumerable<BedViewModel>> GetMonitorList()
        {
            return Task.FromResult(mapper.Map<IEnumerable<BedViewModel>>(GetBedForUser()));
        }

        public Task<IDictionary<string, ICollection<AlertInfo>>> GetAlertRecords()
        {
            var beds = GetBedForUser();
            var filtered = !Context.User.IsInRole(Roles.PowerAdmin) ?
                monitorPool.Alerts.Where(x => beds.Any(b => b.MacAddress == x.Key)).ToDictionary(k => k.Key, v => v.Value)
                : monitorPool.Alerts;
            return Task.FromResult(filtered);
        }

        private IEnumerable<BedBoxInfo> GetBedForUser()
        {
            var units = Context.User.GetUnitList();
            bool isMulti = units.Count() > 1;
            var beds = monitorPool.BedList;
            if (isMulti)
            {
                beds = beds.Where(x => !x.UnitId.HasValue || units.Contains(x.UnitId.Value));
            }
            else if (units.Any()) // skip root admin case
            {
                int unitId = units.First();
                beds = beds.Where(x => x.UnitId.HasValue && unitId == x.UnitId.Value);
            }

            return beds.ToList();
        }

        public Task<PatientInfo> GetPatientInfo(string id)
        {
            var patient = patientService.GetPatient(id);
            if (patient == null)
            {
                return Task.FromResult<PatientInfo>(null);
            }
            return Task.FromResult(new PatientInfo
            {
                Id = patient.Id,
                Name = patient.Name,
                Rfid = patient.RFID ?? ""
            });
        }

        public async Task<NotificationResult> GetLatestNotifications(string culture = null)
        {
            var cultureInfo = string.IsNullOrEmpty(culture) ? null : CultureInfo.GetCultureInfo(culture);
            var userResult = userInfoService.FindUser(x => x.Id == new Guid(Context.User.GetUserId()));
            var (items, total) = await redisPool.GetNotifications(userResult.User, userResult.Roles);
            return new NotificationResult
            {
                Data = items.Select(x => x.ReplaceText(localizer, cultureInfo)),
                Total = total
            };
        }

        public async Task<NotificationResult> GetNotifications(int page = 1, int max = 15, string culture = null)
        {
            var cultureInfo = string.IsNullOrEmpty(culture) ? null : CultureInfo.GetCultureInfo(culture);
            var userResult = userInfoService.FindUser(x => x.Id == new Guid(Context.User.GetUserId()));
            var (items, total) = await redisPool.GetNotifications(userResult.User, userResult.Roles, max, page);

            return new NotificationResult
            {
                Data = items.Select(x => x.ReplaceText(localizer, cultureInfo)),
                Total = total
            };
        }

        public async Task<object> GetOldestNotiCount()
        {
            try
            {
                var userResult = userInfoService.FindUser(x => x.Id == new Guid(Context.User.GetUserId()));
                var (oldestRemoveDate, upperLimit, count) = await redisPool.GetOldestNotiCount(userResult.User, userResult.Roles, tz);

                return new
                {
                    Oldest = new DateTimeOffset(oldestRemoveDate, TimeSpan.Zero),
                    UpperLimit = new DateTimeOffset(upperLimit, TimeSpan.Zero),
                    Count = count
                };
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to get oldest noti count.");

                return new
                {
                    Oldest = DateTimeOffset.MinValue,
                    UpperLimit = DateTimeOffset.MinValue,
                    Count = 0
                };
            }
        }

        public override Task OnConnectedAsync()
        {
            Log.Information($"Client connected. [{Context.ConnectionId} || userId: {Context.User.GetUserId()}]");
            var units = Context.User.GetUnitList();
            bool isMulti = !units.Any() || units.Count() > 1;
            if (isMulti)
            {
                Groups.AddToGroupAsync(Context.ConnectionId, NO_UNIT_CHANNEL); // subscribe to channel for new box with no unit set
            }

            // root admin case
            if (!units.Any())
            {
                Groups.AddToGroupAsync(Context.ConnectionId, ROOT_ADMIN);
            }
            foreach (var unitId in units)
            {
                Groups.AddToGroupAsync(Context.ConnectionId, GetUnitChannelName(unitId)); // subscribe to unit exclusive channel
            }
            var user = userInfoService.FindUser(x => x.Id == new Guid(Context.User.GetUserId()));
            foreach (var role in user.Roles)
            {
                Groups.AddToGroupAsync(Context.ConnectionId, GetRoleChannel(role)); // subscribe to role exclusive channel
                foreach (var unitId in units)
                {
                    Groups.AddToGroupAsync(Context.ConnectionId, GetUnitRoleChannel(unitId, role)); // subscribe to specific group channels
                }
            }

            // also add to personal exclusive group
            Groups.AddToGroupAsync(Context.ConnectionId, Context.User.GetUserId());

            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(System.Exception exception)
        {
            Log.Information($"Client disconnected. [{Context.ConnectionId} || userId: {Context.User.GetUserId()}]");

            return base.OnDisconnectedAsync(exception);
        }

        // ============== Utils =====================

        /// <summary>
        /// Check permission to send command to HemoBox. Only root admin can manipulate a box that has no unit assigned.
        /// </summary>
        /// <param name="bed"></param>
        private bool CheckUnitPermission(BedBoxInfo bed)
        {
            var units = Context.User.GetUnitList();
            return !units.Any() || (bed.UnitId.HasValue && units.Contains(bed.UnitId.Value));
        }
    }

    public class DataRequest
    {
        public string PatientId { get; set; }
        public string MacAddress { get; set; }
    }
}
