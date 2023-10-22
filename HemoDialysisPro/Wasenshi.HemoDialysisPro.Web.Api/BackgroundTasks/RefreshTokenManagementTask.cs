using Hangfire;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Wasenshi.HemoDialysisPro.Jobs;
using Wasenshi.HemoDialysisPro.Models.TimezoneUtil;

namespace Wasenshi.HemoDialysisPro.Web.Api.BackgroundTasks
{
    public class RefreshTokenManagementTask : BackgroundService
    {
        private TimeZoneInfo tz;
        private readonly IConfiguration config;
        private readonly IRecurringJobManager recurringJob;

        public static readonly string REFRESH_TOKEN_CLEAR = "refresh-token-clear";

        public RefreshTokenManagementTask(IConfiguration config, IRecurringJobManager recurringJob)
        {
            this.config = config;
            this.recurringJob = recurringJob;

            tz = TimezoneUtils.GetTimeZone(config.GetValue<string>("TIMEZONE"));
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var interval = config["refresh_token:clear_interval"];
            var ts = TimeSpan.ParseExact(interval, new[] { "d'D'", "h'H'" }, CultureInfo.InvariantCulture);
            Log.Information("refesh token clear interval : {0}", ts);

            recurringJob.AddOrUpdate<RefreshTokenJob>(REFRESH_TOKEN_CLEAR, x => x.PrunRefreshToken(), GetCronInterval(interval),
                new RecurringJobOptions
                {
                    TimeZone = tz
                });

            return Task.CompletedTask;
        }

        private string GetCronInterval(string durationString)
        {
            try
            {
                var n = int.Parse(durationString[0..^1]);
                char type = durationString[^1];
                switch (char.ToLower(type))
                {
                    case 'm':
                        return $"0 0 * */{n} *";
                    case 'h':
                        return $"0 */{n} * * *";
                    case 'd':
                        return $"0 0 */{n} * *";
                    default:
                        return $"0 0 */{n} * *";
                }
            }
            catch (Exception e)
            {
                Log.Error(e, $"Error when parsing duration. [{durationString}] (use default duration: every month)");
                return $"0 * * */1 *";
            }
        }
    }
}
