using Hangfire;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.Threading;
using System.Threading.Tasks;
using Wasenshi.HemoDialysisPro.Jobs;
using Wasenshi.HemoDialysisPro.Models.TimezoneUtil;

namespace Wasenshi.HemoDialysisPro.Web.Api.BackgroundTasks
{
    public class ScheduleManagementTask : BackgroundService
    {
        public string ConsumerTag => consumerTag;
        private string consumerTag;


        private readonly IConfiguration config;
        private readonly IRecurringJobManager recurringJobManager;
        private TimeZoneInfo tz;

        public static readonly string INCHARGE_CLEAR = "incharge-clear";

        public ScheduleManagementTask(IConfiguration config, IRecurringJobManager recurringJobManager)
        {
            this.config = config;
            this.recurringJobManager = recurringJobManager;
            tz = TimezoneUtils.GetTimeZone(config.GetValue<string>("TIMEZONE"));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Log.Logger.Information("Schedule Management Task running.");

            recurringJobManager.AddOrUpdate<ScheduleManageJob>(INCHARGE_CLEAR, x => x.ClearIncharge(), Cron.Monthly(), new RecurringJobOptions { TimeZone = tz });
        }
    }
}
