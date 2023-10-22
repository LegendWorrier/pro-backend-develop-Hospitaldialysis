using AutoMapper;
using Hangfire;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using ServiceStack.Messaging;
using ServiceStack.Redis;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Models.Settings;
using Wasenshi.HemoDialysisPro.Services;
using Wasenshi.HemoDialysisPro.Services.Interfaces;
using Wasenshi.HemoDialysisPro.Share;
using Wasenshi.HemoDialysisPro.Share.GlobalEvents;
using Wasenshi.HemoDialysisPro.Share.RPCs;
using Wasenshi.HemoDialysisPro.Models.ConfigExtesions;
using Wasenshi.HemoDialysisPro.Models.TimezoneUtil;
using Wasenshi.HemoDialysisPro.Web.Api.WebSocket;

namespace Wasenshi.HemoDialysisPro.Web.Api.BackgroundTasks
{
    public class HemosheetManagementTask : BackgroundService
    {
        private TimeZoneInfo tz;
        public IServiceProvider Services { get; }

        // auto fill record
        private static readonly ConcurrentDictionary<Guid, ServiceEvents.DialysisRecordEvent> Listeners = new ConcurrentDictionary<Guid, ServiceEvents.DialysisRecordEvent>();
        private readonly IMessageService mq;
        private static HemosheetManagementTask _instance;

        public HemosheetManagementTask(IConfiguration config, IServiceProvider services, IMessageService mq)
        {
            Services = services;
            this.mq = mq;
            _instance = this;

            tz = TimezoneUtils.GetTimeZone(config.GetValue<string>("TIMEZONE"));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Log.Information("Hemosheet Management Task running.");

            // sent from service on this instance, dispatch it to all.
            ServiceEvents.OnHemosheetCreated += OnNewHemosheet;
            // sent from service on this instance, dispatch it to all.
            ServiceEvents.OnHemosheetCompleted += OnHemosheetComplete;

            ServiceEvents.OnDialysisRecordCreated += OnDialysisRecord;


            // received from message queue, setup the listener for first record
            mq.RegisterHandler<NewHemosheet>(m =>
            {
                var hemoId = m.GetBody().HemoId;
                OnNewHemosheetSignal(hemoId);
                return null;
            });

            // received from message queue, remove the listener for first record. (another instance has already handled the timer setup)
            mq.RegisterHandler<FirstDialysisRecord>(m =>
            {
                var hemoId = m.GetBody().HemoId;
                OnFirstRecordSignal(hemoId);
                return null;
            });

            // handle RPC call
            mq.RegisterHandler<GetDialysisData>(m =>
            {
                var connectionId = m.GetBody().ConnectionId;
                var data = GetDataAsync(Services, connectionId).ConfigureAwait(false).GetAwaiter().GetResult();

                return Message.Create(new GetDialysisDataResponse { Data = data });
            });
        }

        public static async Task<DialysisRecord> GetDataAsync(IServiceProvider services, string connectionId)
        {
            using (var scope = services.CreateScope())
            {
                var hemoboxHub = scope.ServiceProvider.GetRequiredService<IHubContext<HemoBoxHub, IHemoBoxClient>>();
                var hemoService = scope.ServiceProvider.GetRequiredService<IHemoService>();
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var recordService = scope.ServiceProvider.GetRequiredService<IRecordService>();
                var monitorPool = scope.ServiceProvider.GetRequiredService<IRedisClient>().GetMonitorPool();
                (var manual, _) = await HemoBoxHub.GetDataWithResponseAsync(new GetDataRequiredServices
                {
                    HemoService = hemoService,
                    HubContext = hemoboxHub,
                    Mapper = mapper,
                    RecordService = recordService,
                    Monitor = monitorPool
                }, connectionId);

                return manual;
            }
        }

        // ===================== Auto Fill Record ================================

        private void OnDialysisRecord(DialysisRecord record)
        {
            using var scope = Services.CreateScope();
            var redis = scope.ServiceProvider.GetService<IRedisClient>();
            if (redis.ContainsKey(Common.GetHemosheetKey(record.HemodialysisId)))
            {
                redis.SetEntryInHash(Common.GetHemosheetKey(record.HemodialysisId),
                    record.IsFromMachine ? Common.LAST_MACHINE_SAVE_TIME : Common.LAST_RECORD_SAVE_TIME,
                    DateTime.UtcNow.Ticks.ToString());
            }
        }

        private void OnHemosheetComplete(HemodialysisRecord hemosheet)
        {
            Log.Information($"hemosheet has been completed. [{hemosheet.Id}]");

            // untrack the hemosheet
            using var scope = Services.CreateScope();
            var redis = scope.ServiceProvider.GetRequiredService<IRedisClient>();
            redis.Remove(Common.GetHemosheetKey(hemosheet.Id));
            redis.RemoveSession(hemosheet);

            var bgJob = scope.ServiceProvider.GetRequiredService<IBackgroundJobClient>();
            bgJob.ClearAutoComplete(hemosheet.Id, redis);
            bgJob.StopAutoFill(hemosheet.Id);

            var message = scope.ServiceProvider.GetService<IMessageQueueClient>();
            message.Publish(new HemosheetCompleted { HemoId = hemosheet.Id });
        }

        private void OnNewHemosheet(HemodialysisRecord hemosheet)
        {
            Log.Information($"New hemosheet [{hemosheet.Id}] for patient [{hemosheet.PatientId}]");

            // save and track hemosheet
            using var scope = Services.CreateScope();
            var redis = scope.ServiceProvider.GetRequiredService<IRedisClient>();
            redis.SetEntryInHash(Common.GetHemosheetKey(hemosheet.Id), Common.LAST_RECORD_SAVE_TIME, "");
            bool success = redis.AddNewSession(hemosheet);
            if (!success)
            {
                Log.Warning($"Set New Session Failed. There is already on-going session for this patient ID: {hemosheet.PatientId} (hemoId: {redis.GetHemoKeyForCurrentSession(hemosheet.PatientId)})");
            }

            var setting = scope.ServiceProvider.GetRequiredService<IWritableOptions<GlobalSetting>>().Value;
            var hemoSetting = setting.Hemosheet;
            // init auto complete
            SetupAutoComplete(hemosheet, scope, hemoSetting);
            // init auto med
            SetupAutoMedicine(hemosheet, scope, hemoSetting);

            var message = scope.ServiceProvider.GetRequiredService<IMessageQueueClient>();
            message.Publish(new NewHemosheet { HemoId = hemosheet.Id });
            // TODO: check if this is fan out logic or not?

            SetupAutoSchedule(hemosheet, scope, setting.Schedule);
        }

        private void OnNewHemosheetSignal(Guid id)
        {
            var setting = Services.GetRequiredService<IWritableOptions<GlobalSetting>>().Value.Hemosheet;
            if (string.IsNullOrEmpty(setting.Basic.AutoFillRecord))
            {
                return;
            }
            Console.WriteLine("New hemosheet! setup listener...");
            ServiceEvents.DialysisRecordEvent listener = (record) =>
            {
                if (record.HemodialysisId == id)
                {
                    Log.Information($"First dialysis record for hemosheet [{id}], start the auto fill task...");
                    using (var scope = Services.CreateScope())
                    {
                        var setting = scope.ServiceProvider.GetRequiredService<IWritableOptions<GlobalSetting>>().Value.Hemosheet;
                        var interval = GetDuration(setting.Basic.AutoFillRecord);
                        Console.WriteLine($"hemosheet auto fill interval = {interval}");

                        var bgJob = scope.ServiceProvider.GetRequiredService<IBackgroundJobClient>();
                        bgJob.StartAutoFillForHemosheet(id, interval);

                        if (record.IsFromMachine && !string.IsNullOrEmpty(record.Number))
                        {
                            var hemoService = scope.ServiceProvider.GetRequiredService<IHemoService>();
                            var hemosheet = hemoService.GetHemodialysisRecord(id);
                            hemosheet.Bed = record.Number;
                            hemosheet.IsSystemUpdate = true;
                            hemoService.EditHemodialysisRecord(hemosheet);
                        }

                        // Notify all hemo server instance(s) to remove listener for this hemosheet
                        var message = scope.ServiceProvider.GetRequiredService<IMessageQueueClient>();
                        message.Publish(new FirstDialysisRecord { HemoId = id });
                        // TODO: check if this is fan out logic or not?
                    }
                }
            };
            ServiceEvents.OnDialysisRecordCreated += listener;
            Listeners.TryAdd(id, listener);
        }

        private void OnFirstRecordSignal(Guid id)
        {
            Console.WriteLine($"Remove listener for hemoId: {id}");
            if (Listeners.TryRemove(id, out var listener))
            {
                ServiceEvents.OnDialysisRecordCreated -= listener;
            }
        }


        // ======================== Auto Complete =======================

        public static void SetAutoComplete(Guid hemoId)
        {
            if (_instance == null)
            {
                Log.Logger.Error("Cannot find hemo task instance.");
                return;
            }
            _instance.SetupAutoComplete(hemoId);
        }

        public void SetupAutoComplete(Guid hemoId)
        {
            using var scope = Services.CreateScope();
            var setting = scope.ServiceProvider.GetRequiredService<IWritableOptions<GlobalSetting>>().Value.Hemosheet;
            var autoMode = setting.Basic.Auto;

            if (string.IsNullOrWhiteSpace(autoMode) || autoMode == "none")
            {
                return;
            }

            var hemoService = scope.ServiceProvider.GetService<IHemoService>();
            var record = hemoService.GetHemodialysisRecord(hemoId);
            SetupAutoComplete(record, scope, setting);
        }

        public void SetupAutoComplete(HemodialysisRecord record, IServiceScope scope, HemosheetSetting setting)
        {
            using var localScope = Services.CreateScope();
            if (scope == null)
            {
                scope = localScope;
            }
            if (setting == null)
            {
                setting = scope.ServiceProvider.GetRequiredService<IWritableOptions<GlobalSetting>>().Value.Hemosheet;
            }

            var autoMode = setting.Basic.Auto;

            if (string.IsNullOrWhiteSpace(autoMode) || autoMode == "none")
            {
                return;
            }

            TimeSpan duration = record.DialysisPrescription?.Duration ?? TimeSpan.Zero;
            if (record.DialysisPrescriptionId.HasValue && duration == TimeSpan.Zero)
            {
                var hemoService = scope.ServiceProvider.GetRequiredService<IHemoService>();
                var prescription = hemoService.GetDialysisPrescription(record.DialysisPrescriptionId.Value);
                duration = prescription.Duration;
            }

            DateTime targetTime = DateTime.UtcNow;
            if (autoMode == "eod" || (duration == TimeSpan.Zero && autoMode == "duration"))
            {
                var tzNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);
                targetTime = tzNow.AddDays(1).AddTicks(-tzNow.TimeOfDay.Ticks); //  end of day / start of next day
            }
            else if (autoMode == "duration")
            {
                targetTime += duration;
            }

            if (!string.IsNullOrWhiteSpace(setting.Basic.Delay))
            {
                var delay = GetDuration(setting.Basic.Delay);
                targetTime += delay;
            }

            var redis = scope.ServiceProvider.GetRequiredService<IRedisClient>();
            var bgJob = scope.ServiceProvider.GetRequiredService<IBackgroundJobClient>();
            bgJob.QueueAutoComplete(new HemosheetCompleteTask { HemoId = record.Id, PatientId = record.PatientId, TargetDateTime = targetTime }, redis);
        }

        private TimeSpan GetDuration(string durationString)
        {
            try
            {
                var n = int.Parse(durationString[0..^1]);
                char type = durationString[^1];
                switch (char.ToLower(type))
                {
                    case 'm':
                        return TimeSpan.FromMinutes(n);
                    case 'h':
                        return TimeSpan.FromHours(n);
                    default:
                        return TimeSpan.Zero;
                }
            }
            catch (Exception e)
            {
                Log.Error(e, $"Error when parsing duration. [{durationString}] (use default duration: zero)");
                return TimeSpan.Zero;
            }
        }

        // ======================== Auto Medicine =============================

        private static void SetupAutoMedicine(HemodialysisRecord record, IServiceScope scope, HemosheetSetting setting)
        {
            if (setting == null)
            {
                return;
            }
            // auto fill meds
            if (setting.Basic.AutoFillMedicine)
            {
                var bgJob = scope.ServiceProvider.GetRequiredService<IBackgroundJobClient>();
                bgJob.AutoMedicine(record);
            }
        }

        // =========================== Auto Schedule ==============================
        private static void SetupAutoSchedule(HemodialysisRecord record, IServiceScope scope, ScheduleSetting setting)
        {
            if (setting == null)
            {
                return;
            }
            if (setting.AutoSchedule)
            {
                // Auto schedule patient for current session
                var scheduleService = scope.ServiceProvider.GetRequiredService<IScheduleService>();
                var check = scheduleService.PatientCheckForToday(record.PatientId);
                if (!check.HasActiveToday && check.ClosetSlot != null)
                {
                    scheduleService.Reschedule(new[]
                    {
                        new Schedule
                        {
                            PatientId = record.PatientId,
                            IsSystemUpdate = true,
                            Date = DateTime.UtcNow.AddSeconds(10),
                            Slot = check.ClosetSlot.Slot,
                            SectionId = check.ClosetSlot.SectionId
                        }
                    });
                }
            }
        }

        public override void Dispose()
        {
            Log.Information("Disposing hemosheet management task...");
            base.Dispose();
        }
    }
}
