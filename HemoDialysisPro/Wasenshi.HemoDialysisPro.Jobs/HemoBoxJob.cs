using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ServiceStack.Redis;
using System;
using Wasenshi.HemoDialysisPro.Share;
using Wasenshi.HemoDialysisPro.Models.TimezoneUtil;

namespace Wasenshi.HemoDialysisPro.Jobs
{
    public class HemoBoxJob
    {
        public readonly IServiceProvider Services;

        private TimeZoneInfo tz;

        public HemoBoxJob(IServiceProvider services, IConfiguration config)
        {
            this.Services = services;

            tz = TimezoneUtils.GetTimeZone(config["TIMEZONE"]);
        }

        public void ClearAlert()
        {
            using var scope = Services.CreateScope();
            var redis = scope.ServiceProvider.GetRequiredService<IRedisClient>();
            redis.GetMonitorPool().ClearAlertRecord();
        }

    }
}
