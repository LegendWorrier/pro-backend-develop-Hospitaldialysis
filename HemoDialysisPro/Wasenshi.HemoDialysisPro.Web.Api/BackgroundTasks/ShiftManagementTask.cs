using Hangfire;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using ServiceStack.Redis;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Wasenshi.HemoDialysisPro.Jobs;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Repositories;
using Wasenshi.HemoDialysisPro.Services.Interfaces;
using Wasenshi.HemoDialysisPro.Share;
using Wasenshi.HemoDialysisPro.Models.TimezoneUtil;

namespace Wasenshi.HemoDialysisPro.Web.Api.BackgroundTasks
{
    public class ShiftManagementTask : BackgroundService
    {
        private TimeZoneInfo tz;

        private bool isInitPhrase;
        private readonly IConfiguration config;
        private readonly IRecurringJobManager recurringJobManager;
        private readonly IBackgroundJobClient backgroundJobClient;

        public IServiceProvider Services { get; }



        public static readonly string SHIFT_TABLE_CLEAR = "shift-table-clear";

        public ShiftManagementTask(
            IConfiguration config,
            IRecurringJobManager recurringJobManager,
            IBackgroundJobClient backgroundJobClient,
            IServiceProvider services)
        {
            this.config = config;
            this.recurringJobManager = recurringJobManager;
            this.backgroundJobClient = backgroundJobClient;

            Services = services;
            tz = TimezoneUtils.GetTimeZone(config.GetValue<string>("TIMEZONE"));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Log.Information("Shift Management Task running.");

            recurringJobManager.AddOrUpdate<ShiftManagementJob>(SHIFT_TABLE_CLEAR, x => x.ClearHistory(), Cron.Monthly(), new RecurringJobOptions { TimeZone = tz });

            using (var scope = Services.CreateScope())
            {
                // first time init? (if db is not setup yet, skip this part)
                bool isDatabaseInit = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Database.CanConnect();
                bool isInitMigration = config.GetValue<bool>("INIT");

                isInitPhrase = !isDatabaseInit || isInitMigration;
                if (!isInitPhrase)
                {
                    var units = scope.ServiceProvider.GetRequiredService<IMasterDataService>().GetMasterDataList<Unit>().Take(LicenseManager.MaxUnits);

                    var tzNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);
                    Log.Information($"current local time: {tzNow:T}");
                    foreach (var unit in units)
                    {
                        backgroundJobClient.Enqueue<ShiftManagementJob>(x => x.UpdateUnitMeta(unit.Id));
                    }
                }
                else
                {
                    if (!isDatabaseInit)
                    {
                        var redis = scope.ServiceProvider.GetRequiredService<IRedisClient>();
                        redis.UpdateUnitShift(-1, new UnitShift
                        {
                            Id = -1,
                            CurrentShift = -1,
                        });
                        redis.As<Unit>().Store(new Unit
                        {
                            Id = -1,
                            Name = "Unit 1"
                        });
                    }
                }
            }
        }
    }
}
