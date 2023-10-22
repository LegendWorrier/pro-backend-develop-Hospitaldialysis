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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Wasenshi.HemoDialysisPro.Jobs;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Repositories;
using Wasenshi.HemoDialysisPro.Services;
using Wasenshi.HemoDialysisPro.Services.Interfaces;
using Wasenshi.HemoDialysisPro.Share;
using Wasenshi.HemoDialysisPro.Share.GlobalEvents;
using Wasenshi.HemoDialysisPro.Models.TimezoneUtil;
using Wasenshi.HemoDialysisPro.Web.Api.WebSocket;

namespace Wasenshi.HemoDialysisPro.Web.Api.BackgroundTasks
{
    public class HemoBoxManagementTask : BackgroundService
    {
        private Timer _workTimer;
        private static HemoBoxManagementTask instance_;
        private readonly IServiceProvider services;
        private readonly IConfiguration config;
        private readonly IRecurringJobManager recurringJob;
        private readonly IMessageService mq;

        private TimeZoneInfo tz;

        public static readonly string ALERT_CLEAR = "alert-clear";

        public HemoBoxManagementTask(IServiceProvider services, IConfiguration config, IRecurringJobManager recurring, IMessageService mq)
        {
            this.services = services;
            this.config = config;
            this.recurringJob = recurring;
            this.mq = mq;
            instance_ = this;

            tz = TimezoneUtils.GetTimeZone(config["TIMEZONE"]);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            recurringJob.AddOrUpdate<HemoBoxJob>(ALERT_CLEAR, x => x.ClearAlert(), Cron.Daily(), new RecurringJobOptions { TimeZone = tz });

            using var scope = services.CreateScope();

            // first time init? (if db is not setup yet, skip this part)
            bool isDatabaseInit = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Database.CanConnect();
            bool isInitMigration = config.GetValue<bool>("INIT");

            var isInitPhrase = !isDatabaseInit || isInitMigration;
            if (!isInitPhrase)
            {
                var redis = scope.ServiceProvider.GetRequiredService<IRedisClient>();
                var master = scope.ServiceProvider.GetRequiredService<IMasterDataService>();
                redis.As<Unit>().StoreAll(master.GetMasterDataList<Unit>().Take(LicenseManager.MaxUnits)); // access database only first time, then cach it on redis
                redis.GetMonitorPool().ClearConnectionList();
            }

            mq.RegisterHandler<UnitUpdated>(m =>
            {
                using var scope = services.CreateScope();
                var redis = scope.ServiceProvider.GetRequiredService<IRedisClient>();
                MonitorPool.UpdateUnitCache(m, redis);
                return null;
            }); // update cache on redis on-the-fly

            ServiceEvents.OnPatientIdUpdated += OnPatientIdUpdated;

            _workTimer = new Timer(async (o) => await DoWork(o), null, TimeSpan.Zero, TimeSpan.FromSeconds(5));
        }

        private void OnPatientIdUpdated(string oldId, string newId)
        {
            HemoBoxQueue.AddWorkToQueue(async (scope) =>
            {
                var hub = scope.ServiceProvider.GetRequiredService<IHubContext<HemoBoxHub, IHemoBoxClient>>();
                var redis = scope.ServiceProvider.GetRequiredService<IRedisClient>();

                // Update on-going HemoBox also when patient ID got updated
                var monitorPool = redis.GetMonitorPool();
                var bed = monitorPool.GetBedByPatientId(oldId);

                if (bed != null)
                {
                    bed.PatientId = newId;
                    bed.Patient.Id = newId;
                    monitorPool.AddOrUpdateBed(bed);

                    var userHub = scope.ServiceProvider.GetRequiredService<IHubContext<UserHub, IUserClient>>();
                    var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                    await HemoBoxHubHelpers.DispatchHemoBoxUpdateEvent(bed, userHub, mapper, t => t.BedPatient(bed.MacAddress, bed.Patient));

                    if (bed.ConnectionId != null)
                    {
                        await hub.Clients.Client(bed.ConnectionId).PatientIdChanged(oldId, newId);
                    }
                }
            });
            HemoBoxQueue.StartImmediately();
        }

        public async Task DoWork(object state)
        {
            while (HemoBoxQueue.Queue.TryDequeue(out var work))
            {
                using var scope = services.CreateScope();
                try
                {
                    await work(scope);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, $"Failed to execute HemoBox queue.");
                }
                finally
                {
                    scope.Dispose();
                }
            }
        }

        public static void TrickerQueueTimer()
        {
            instance_?._workTimer.Change(Timeout.Infinite, Timeout.Infinite); // stop
            instance_?._workTimer.Change(TimeSpan.Zero, TimeSpan.FromSeconds(5)); // start again
        }

        public override void Dispose()
        {
            Log.Information("Disposing HemoBox management task...");
            base.Dispose();
        }
    }

    /// <summary>
    /// Simple queue that will only involve the current hemoserver instance.
    /// </summary>
    public static class HemoBoxQueue
    {
        internal static readonly ConcurrentQueue<Func<IServiceScope, Task>> Queue = new ConcurrentQueue<Func<IServiceScope, Task>>();

        public static void AddWorkToQueue(Func<IServiceScope, Task> work)
        {
            Queue.Enqueue(work);
        }

        public static void StartImmediately()
        {
            HemoBoxManagementTask.TrickerQueueTimer();
        }
    }
}
