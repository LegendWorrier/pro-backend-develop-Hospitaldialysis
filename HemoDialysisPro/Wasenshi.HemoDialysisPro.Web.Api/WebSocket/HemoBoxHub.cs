using AutoMapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using ServiceStack.Messaging;
using ServiceStack.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Services.Interfaces;
using Wasenshi.HemoDialysisPro.Share;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.ViewModels;
using Wasenshi.HemoDialysisPro.Web.Api.BackgroundTasks;

namespace Wasenshi.HemoDialysisPro.Web.Api.WebSocket
{
    public class HemoBoxHub : Hub<IHemoBoxClient>
    {
        private readonly IConfiguration config;
        private readonly IWebHostEnvironment webHost;
        private readonly IUploadService uploadService;
        private readonly IHemoService hemoService;
        private readonly IRecordService recordService;
        private readonly IPatientService patientService;
        private readonly IUserManagementService userService;
        private readonly IMasterDataService master;
        private readonly IRedisClient redis;
        private readonly IMessageQueueClient message;
        private readonly MonitorPool monitorPool;
        private readonly IHubContext<UserHub, IUserClient> userHub;
        private readonly IMapper mapper;

        private static readonly Dictionary<string, object> ResponseTasks = new Dictionary<string, object>();

        public HemoBoxHub(
            IConfiguration config,
            IWebHostEnvironment webHost,
            IUploadService uploadService,
            IHemoService hemoService,
            IRecordService recordService,
            IPatientService patientService,
            IUserManagementService userService,
            IMasterDataService master,
            IRedisClient redis,
            IMessageQueueClient message,
            IHubContext<UserHub, IUserClient> userHub,
            IMapper mapper)
        {
            this.config = config;
            this.webHost = webHost;
            this.uploadService = uploadService;
            this.hemoService = hemoService;
            this.recordService = recordService;
            this.patientService = patientService;
            this.userService = userService;
            this.master = master;
            this.redis = redis;
            this.message = message;
            this.userHub = userHub;
            this.mapper = mapper;

            monitorPool = redis.GetMonitorPool();
        }
        // ================================= Callback methods =================================
        public async Task<bool> Register(RegisterBoxReq req, string cid)
        {
            Log.Information("[HEMOBOX] Get register info from hemobox");
            if (string.IsNullOrEmpty(cid) || !ResponseTasks.ContainsKey(cid))
            {
                Log.Information("[HEMOBOX] This register is not initiated by the server, or has timeouted earlier.");
                await Groups.AddToGroupAsync(Context.ConnectionId, req.MacAddress);
                var bed = HemoBoxHubHelpers.UpdateBedWithNewConnectionId(req, Context.ConnectionId, HemoBoxHubHelpers.GetServiceArg(monitorPool, master));
                await _DispatchHemoBoxRegisterEvent(Context.ConnectionId, req, userHub, bed, mapper);
                return true;
            }

            var taskCompleteSource = (TaskCompletionSource<RegisterBoxReq>)ResponseTasks[cid];
            if (!taskCompleteSource.TrySetResult(req))
            {
                Log.Information("Cannot set result for register box.");
                return false;
            }

            return true;
        }

        public async Task<bool> DataResponse(DialysisData data, string cid)
        {
            Log.Information("[HEMOBOX] Get data from hemobox");
            if (string.IsNullOrEmpty(cid) || !ResponseTasks.ContainsKey(cid))
            {
                Log.Information("[HEMOBOX] This data is not initiated by the server, or has timeouted earlier.");
                return false;
            }

            var taskCompleteSource = (TaskCompletionSource<DialysisData>)ResponseTasks[cid];
            if (!taskCompleteSource.TrySetResult(data))
            {
                Log.Information("Cannot set result for data response.");
                return false;
            }

            return true;
        }

        public static async Task<(DialysisRecord, HemodialysisRecord)> GetDataWithResponseAsync(GetDataRequiredServices services, string connectionId)
        {
            var monitorPool = services.Monitor;
            var hubContext = services.HubContext;
            var hemoService = services.HemoService;
            var recordService = services.RecordService;
            var mapper = services.Mapper;
            var bed = monitorPool.GetBedByConnectionId(connectionId);
            if (bed == null)
            {
                throw new HubException($"LOST:the target hemobox connection may have been lost.");
            }
            IHemoBoxClient targetBox = hubContext.Clients.Client(connectionId);
            var data = await CallWithResponse<DialysisData>((cid) => targetBox.GetData(cid), connectionId);

            if (data == null)
            {
                return (null, null);
            }

            var result = _PushDialysisData(data, bed.PatientId, hemoService, recordService, mapper);

            return result;
        }

        // ==============================================================

        public Task<IEnumerable<UnitInfo>> GetUnitList()
        {
            var unitList = monitorPool
                .UnitListFromCache()
                .OrderBy(x => x.Id)
                .Select(x => new UnitInfo { Id = x.Id, Name = x.Name });

            return Task.FromResult(unitList);
        }

        public PatientInfo PickPatient(PatientReq req)
        {
            var patient = req.Rfid ? patientService.GetPatientByRFID(req.Id) : patientService.GetPatient(req.Id);
            if (patient == null)
            {
                return null;
            }

            float weight = 0;
            var hemosheet = hemoService.GetHemodialysisRecordByPatientId(req.Id);
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

            var bed = monitorPool.GetBedByConnectionId(Context.ConnectionId);
            bed.PatientId = patient.Id;
            bed.Patient = result;
            monitorPool.AddOrUpdateBed(bed);

            var connectionId = Context.ConnectionId;
            HemoBoxQueue.AddWorkToQueue(async (scope) =>
            {
                var userHub = scope.ServiceProvider.GetRequiredService<IHubContext<UserHub, IUserClient>>();
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                await HemoBoxHubHelpers.DispatchHemoBoxUpdateEvent(bed, userHub, mapper, t => t.BedPatient(bed.MacAddress, result));
            });
            HemoBoxQueue.StartImmediately();

            return result;
        }

        public string PushDialysisData(DialysisData data)
        {
            var bed = monitorPool.GetBedByConnectionId(Context.ConnectionId);
            string patientId = bed.PatientId;

            (var record, var _) = _PushDialysisData(data, patientId, hemoService, recordService, mapper);

            if (record == null)
            {
                return null;
            }
            return record.Id.ToString();
        }

        public async Task Alert(Alarm alarm)
        {
            var bed = monitorPool.GetBedByConnectionId(Context.ConnectionId);

            monitorPool.AddAlert(bed.MacAddress, alarm);

            await _DispatchHemoBoxAlertEvent(Context.ConnectionId, bed, alarm, userHub);
        }

        public async Task ChangeName(string name)
        {
            Log.Information("[HEMOBOX] hemobox change name: " + name);

            var bed = monitorPool.GetBedByConnectionId(Context.ConnectionId);
            bed.Name = name;
            monitorPool.AddOrUpdateBed(bed);

            await HemoBoxHubHelpers.DispatchHemoBoxUpdateEvent(bed, userHub, mapper);
        }

        public async Task Finish()
        {
            Log.Information("[HEMOBOX] hemobox finishes its session, signal to remove patient.");

            var bed = monitorPool.GetBedByConnectionId(Context.ConnectionId);
            bed.Patient = null;
            bed.PatientId = null;
            monitorPool.AddOrUpdateBed(bed);

            await HemoBoxHubHelpers.DispatchHemoBoxUpdateEvent(bed, userHub, mapper);
        }

        public async Task UpdateStatus(BoxStatus status)
        {
            var bed = monitorPool.GetBedByConnectionId(Context.ConnectionId);
            Log.Information($"[HEMOBOX] hemobox status: {status} [MAC: {bed.MacAddress}, cid: {Context.ConnectionId}]");
            if ((int)status > 1)
            {
                bed.Online = status == BoxStatus.Online;
            }
            else
            {
                bed.Sending = status == BoxStatus.Sending;
            }
            monitorPool.AddOrUpdateBed(bed);

            await HemoBoxHubHelpers.DispatchHemoBoxUpdateEvent(bed, userHub, mapper, t => t.BoxStatus(bed.MacAddress, status));
        }

        public async Task UpdateUnit(int unitId)
        {
            var bed = monitorPool.GetBedByConnectionId(Context.ConnectionId);
            Log.Information($"[HEMOBOX] hemobox change to unit: {unitId} [MAC: {bed.MacAddress}, cid: {Context.ConnectionId}]");

            if (bed.UnitId.HasValue)
            {
                await userHub.Clients.Group(UserHub.GetUnitChannelName(bed.UnitId.Value)).BoxChangeUnit(bed.MacAddress, unitId);
            }

            bed.UnitId = unitId;
            monitorPool.AddOrUpdateBed(bed);

            await HemoBoxHubHelpers.DispatchHemoBoxUpdateEvent(bed, userHub, mapper);
        }

        public override async Task OnConnectedAsync()
        {
            Log.Information($"[HEMOBOX] a hemobox has connected. [{Context.ConnectionId}]");

            await base.OnConnectedAsync();

            var connectionId = Context.ConnectionId;
            HemoBoxQueue.AddWorkToQueue(async (scope) =>
            {
                var hemoBoxHubContext = scope.ServiceProvider.GetRequiredService<IHubContext<HemoBoxHub, IHemoBoxClient>>();
                var monitorPool = scope.ServiceProvider.GetRequiredService<IRedisClient>().GetMonitorPool();
                var client = hemoBoxHubContext.Clients.Client(connectionId);
                var info = await CallWithResponse<RegisterBoxReq>((cid) => client.GetInfo(cid), connectionId);
                if (info != null)
                {
                    Log.Information($"[HEMOBOX] info: {JsonSerializer.Serialize(info)}");
                    await hemoBoxHubContext.Groups.AddToGroupAsync(connectionId, info.MacAddress);
                    var existing = monitorPool.GetBedByMacAddress(info.MacAddress);
                    if (existing != null && existing.UnitId.HasValue && existing.UnitId != info.UnitId)
                    {
                        await userHub.Clients.Group(UserHub.GetUnitChannelName(existing.UnitId.Value)).BoxChangeUnit(existing.MacAddress, info.UnitId.GetValueOrDefault());
                    }
                    var master = scope.ServiceProvider.GetRequiredService<IMasterDataService>();
                    var bed = HemoBoxHubHelpers.UpdateBedWithNewConnectionId(info, connectionId, HemoBoxHubHelpers.GetServiceArg(monitorPool, master));
                    var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                    await _DispatchHemoBoxRegisterEvent(connectionId, info, userHub, bed, mapper);
                    // note: the box might be first connect or re-connect here, so we choose to just update the whole data instead of sending only online status.
                }
            });
            HemoBoxQueue.StartImmediately();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            await base.OnDisconnectedAsync(exception);

            if (exception != null)
            {
                Log.Error(exception, $"[HEMOBOX] Error at HemoBoxHub [{Context.ConnectionId}]");
            }
            Log.Information($"[HEMOBOX] a hemobox has disconnected. [{Context.ConnectionId}]");
            var bed = monitorPool.GetBedByConnectionId(Context.ConnectionId);
            if (bed == null)
            {
                Log.Error($"Invalid state! cannot find corresponding box that link with connectionId: {Context.ConnectionId}");
                return;
            }
            bed.PatientId = null;
            bed.Online = false;
            monitorPool.AddOrUpdateBed(bed);

            // notify disconnection in case it's running with patient
            if (bed.UnitId.HasValue && bed.Patient != null && bed.Sending)
            {
                var target = NotificationTarget.ForNurses(bed.UnitId.Value);
                var noti = redis.AddNotification(
                    "box_discon_title",
                    $"box_discon_detail::{bed.Patient.Name}::{(string.IsNullOrWhiteSpace(bed.Name) ? "{noname}" : bed.Name)}",
                    new[] { "page", "monitor" },
                    NotificationType.Warning,
                    target,
                    "box"
                    );
                message.SendNotificationEvent(noti, target);
            }
            await HemoBoxHubHelpers.DispatchHemoBoxUpdateEvent(bed, userHub, mapper, t => t.BoxStatus(bed.MacAddress, BoxStatus.Offline));
        }

        // ===================== Dispatch Utils ============================

        private static async Task _DispatchHemoBoxRegisterEvent(string connectionId, RegisterBoxReq info, IHubContext<UserHub, IUserClient> users, BedBoxInfo bed = null, IMapper mapper = null)
        {
            IUserClient targets = HemoBoxHubHelpers.GetDispatchTarget(info.UnitId, users);
            if (bed != null)
            {
                await targets.BedUpdate(mapper.Map<BedViewModel>(bed));
            }
            else
            {
                await targets.BoxStatus(connectionId, BoxStatus.Online);
            }
        }

        private static async Task _DispatchHemoBoxAlertEvent(string connectionId, BedBoxInfo bed, Alarm alarm, IHubContext<UserHub, IUserClient> users)
        {
            IUserClient targets = HemoBoxHubHelpers.GetDispatchTarget(bed.UnitId, users);
            await targets.BoxAlert(bed.MacAddress, alarm);
        }

        // ====================== private Utils ============================

        private static (DialysisRecord, HemodialysisRecord) _PushDialysisData(DialysisData data, string patientId, IHemoService hemoService, IRecordService recordService, IMapper mapper)
        {
            if (patientId == null)
            {
                return (null, null);
            }
            var hemosheet = hemoService.GetHemodialysisRecordByPatientId(patientId);
            if (hemosheet == null)
            {
                return (null, null);
            }

            // calculate remaining time
            if (data.TotalTime != null && data.Remaining == null)
            {
                data.Remaining = Math.Max(0, (int)(hemosheet.DialysisPrescription?.Duration.TotalMinutes ?? 0) - data.TotalTime.Value);
            }

            var newRecord = mapper.Map<DialysisRecord>(data);
            newRecord.HemodialysisId = hemosheet.Id;
            newRecord.IsFromMachine = true;
            newRecord.IsSystemUpdate = true;

            var dialysis = recordService.CreateDialysisRecord(newRecord);

            return (dialysis, hemosheet);
        }

        /// <summary>
        /// This private utils is intended to be used with callback method which must be manually defined by developer and called by the HemoBox.
        /// The callback method should accept correlationId and finish a corresponding TaskCompletionSource stored in 'ResponseTasks' pool.
        /// <br></br>
        /// <br></br>
        /// (Default timeout for response waiting is '5 seconds')
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="call"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        private static async Task<TResult> CallWithResponse<TResult>(Expression<Action<string>> call, string connectionId)
        {
            if (call.Body.NodeType != ExpressionType.Call || !(call.Body as MethodCallExpression).Object.Type.IsAssignableFrom(typeof(IHemoBoxClient)))
            {
                throw new ArgumentException("The call must be a method invocation on the target HemoClient object.");
            }
            var callParams = (call.Body as MethodCallExpression)?.Method.GetParameters();
            if (callParams?.Length == 0 || callParams?.First().ParameterType != typeof(string))
            {
                throw new ArgumentException("The target method must have at least 1 parameter and the first parameter must be string.");
            }

            // CancellationTokenSource tokenSource = new CancellationTokenSource();
            string correlationId = Guid.NewGuid().ToString();
            var response = new TaskCompletionSource<TResult>();
            ResponseTasks.Add(correlationId, response);

            call.Compile()(correlationId);

            Timer timeout = new Timer((o) => response.TrySetCanceled(), null, TimeSpan.FromSeconds(5), Timeout.InfiniteTimeSpan);

            try
            {
                TResult result = await response.Task;
                return result;
            }
            catch (Exception)
            {
                Log.Information($"No responding from hemobox. [{connectionId}]");
                throw new HubException($"TIMEOUT:No responding from hemobox. [{connectionId}]");
            }
            finally
            {
                ResponseTasks.Remove(correlationId);
            }
        }
    }

    public class RegisterBoxReq
    {
        public string MacAddress { get; set; }
        public string Name { get; set; }
        public int? UnitId { get; set; }
        public string PatientId { get; set; }
        public PatientInfo Patient { get; set; }
        public bool Sending { get; set; }
    }

    public struct PatientReq
    {
        public string Id { get; set; }
        public bool Rfid { get; set; }
    }

    public struct UnitInfo
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public struct GetDataRequiredServices
    {
        public IHemoService HemoService { get; set; }
        public IRecordService RecordService { get; set; }
        public IMapper Mapper { get; set; }
        public MonitorPool Monitor { get; set; }
        public IHubContext<HemoBoxHub, IHemoBoxClient> HubContext { get; set; }
    }
}
