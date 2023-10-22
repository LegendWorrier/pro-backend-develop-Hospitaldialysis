using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ServiceStack.Messaging;
using System;
using Wasenshi.HemoDialysisPro.Services.Interfaces;
using Wasenshi.HemoDialysisPro.Share;
using Wasenshi.HemoDialysisPro.Models.TimezoneUtil;

namespace Wasenshi.HemoDialysisPro.Jobs
{
    public class ScheduleManageJob
    {
        private TimeZoneInfo tz;
        private readonly ILogger<ScheduleManageJob> logger;

        public IServiceProvider Service { get; }

        public ScheduleManageJob(
            IConfiguration config,
            IServiceProvider service,
            ILogger<ScheduleManageJob> logger)
        {
            tz = TimezoneUtils.GetTimeZone(config.GetValue<string>("TIMEZONE"));
            Service = service;
            this.logger = logger;
        }

        public void ApplySectionUpdate(int unitId)
        {
            using (var scope = Service.CreateScope())
            {
                var scheduleService = scope.ServiceProvider.GetRequiredService<IScheduleService>();
                var message = scope.ServiceProvider.GetRequiredService<IMessageQueueClient>();

                scheduleService.ApplyTempSections(unitId);

                logger.LogInformation($"[TASK] Pending section has been applied. [UnitId: {unitId}]");

                // update meta data
                message.Publish(new SectionUpdated { UnitId = unitId });
            }
        }

        public void ClearIncharge()
        {
            using (var scope = Service.CreateScope())
            {
                var shiftService = scope.ServiceProvider.GetRequiredService<IShiftService>();
                shiftService.ClearIncharge();
            }
        }
    }
}
