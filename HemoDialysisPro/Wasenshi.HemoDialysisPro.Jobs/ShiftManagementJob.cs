using Hangfire;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ServiceStack.Messaging;
using ServiceStack.Redis;
using System;
using System.Linq;
using Wasenshi.HemoDialysisPro.Models.Settings;
using Wasenshi.HemoDialysisPro.Services.Interfaces;
using Wasenshi.HemoDialysisPro.Share;
using Wasenshi.HemoDialysisPro.Models.ConfigExtesions;
using Wasenshi.HemoDialysisPro.Models.TimezoneUtil;

namespace Wasenshi.HemoDialysisPro.Jobs
{
    public class ShiftManagementJob
    {
        private TimeZoneInfo tz;
        private readonly IConfiguration config;
        private readonly ILogger<ShiftManagementJob> logger;

        public IServiceProvider Service { get; }

        public static string GetUnitTimerJobId(int unitId) => $"st-{unitId}";

        public ShiftManagementJob(
            IConfiguration config,
            IServiceProvider service,
            ILogger<ShiftManagementJob> logger)
        {
            tz = TimezoneUtils.GetTimeZone(config.GetValue<string>("TIMEZONE"));
            this.config = config;
            Service = service;
            this.logger = logger;
        }

        public static object OnStartNextRound(IMessage<StartNextRound> message)
        {
            int unitId = message.GetBody().UnitId;

            BackgroundJob.Enqueue<ShiftManagementJob>(x => x.StartNextRound(unitId));

            return null;
        }

        public static object OnSectionUpdated(IMessage<SectionUpdated> message)
        {
            int unitId = message.GetBody().UnitId;

            BackgroundJob.Enqueue<ShiftManagementJob>(x => x.UpdateUnitMeta(unitId));

            return null;
        }

        /// <summary>
        /// If current round is already the last round, stop today's last round and prepare for the next day.
        /// </summary>
        /// <param name="unitId"></param>
        public void StartNextRound(int unitId)
        {
            using (var scope = Service.CreateScope())
            {
                var redis = scope.ServiceProvider.GetRequiredService<IRedisClient>();

                var unitShift = redis.GetUnitShift(unitId);
                var sections = unitShift.Sections;

                var lastJobId = redis.Get<string>(GetUnitTimerJobId(unitId));
                if (!string.IsNullOrEmpty(lastJobId))
                {
                    BackgroundJob.Delete(lastJobId); // reset and remove current timer for the unit
                }

                var tzNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);
                var currentTime = TimeOnly.FromTimeSpan(tzNow.TimeOfDay);
                TimeSpan delay;
                var currentShift = ++unitShift.CurrentShift; // move to next round
                if (currentShift >= sections.Count)
                {
                    unitShift.CurrentShift = -1;
                    delay = sections[0].StartTime - currentTime;
                }
                else if (currentShift == sections.Count - 1)
                {
                    var section = unitShift.CurrentSection;
                    var endTime = section.StartTime.AddHours(4);
                    delay = endTime - currentTime;
                }
                else
                {
                    var next = unitShift.NextSection.StartTime;
                    delay = next - currentTime;
                }
                unitShift.LastStarted = DateTime.UtcNow;

                //save changes
                redis.UpdateUnitShift(unitId, unitShift);

                logger.LogDebug($"current local time: {tzNow:T}");
                logger.LogDebug($"Unit: {unitId} | Time until next shift: {delay}");
                var jobId = BackgroundJob.Schedule<ShiftManagementJob>(x => x.StartNextRound(unitId), delay);
                redis.Set(GetUnitTimerJobId(unitId), jobId);
            }
        }

        /// <summary>
        /// Update Unit's meta data. Used when any unit's section has been updated.
        /// </summary>
        /// <param name="unitId"></param>
        public void UpdateUnitMeta(int unitId)
        {
            UnitShift unitShift = new UnitShift
            {
                Id = unitId,
                CurrentShift = -1
            };

            using (var scope = Service.CreateScope())
            {
                var redis = scope.ServiceProvider.GetRequiredService<IRedisClient>();
                var scheduleService = scope.ServiceProvider.GetRequiredService<IScheduleService>();

                using (redis.AcquireLock(UnitShift.GetKey(unitId), TimeSpan.FromSeconds(5)))
                {
                    try
                    {
                        var sections = scheduleService.GetSections(unitId).ToList();

                        var tzNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);
                        var currentTime = TimeOnly.FromTimeSpan(tzNow.TimeOfDay);
                        unitShift.Sections = sections;
                        if (sections.Any())
                        {
                            var lastJobId = redis.Get<string>(GetUnitTimerJobId(unitId));
                            if (!string.IsNullOrEmpty(lastJobId))
                            {
                                BackgroundJob.Delete(lastJobId); // reset and remove current timer for the unit
                            }

                            var lastSectionStartTime = sections.Last().StartTime;
                            var lastSectionEndtime = lastSectionStartTime.AddHours(4);
                            var isWithinLastRound = currentTime.IsBetween(lastSectionStartTime, lastSectionEndtime);

                            TimeSpan delay;
                            string jobId = null;
                            // determind current round and set delay for next round
                            for (int i = 0; i < sections.Count(); i++)
                            {
                                var startTime = sections[i].StartTime;
                                if (currentTime < startTime)
                                {
                                    // if last round is overflow to next day, this may still be the last round
                                    if (isWithinLastRound)
                                    {
                                        unitShift.CurrentShift = sections.Count - 1;
                                        // set timer to stop the last round at the end of last round
                                        delay = lastSectionEndtime - currentTime;
                                    }
                                    else
                                    {
                                        if (i > 0) // if today's rounds have already started
                                        {
                                            unitShift.CurrentShift = i - 1;
                                        }
                                        // set timer for the upcoming round
                                        delay = startTime - currentTime;
                                    }

                                    jobId = BackgroundJob.Schedule<ShiftManagementJob>(x => x.StartNextRound(unitId), delay);

                                    break;
                                }
                            }
                            // in case time is past the start of last round already 
                            if (string.IsNullOrEmpty(jobId))
                            {
                                if (isWithinLastRound) // still last round
                                {
                                    unitShift.CurrentShift = sections.Count - 1;
                                    // set timer to stop the last round at the end of last round
                                    delay = lastSectionEndtime - currentTime;
                                }
                                else
                                {
                                    // start first round at the next day
                                    delay = sections[0].StartTime - currentTime;
                                }

                                jobId = BackgroundJob.Schedule<ShiftManagementJob>(x => x.StartNextRound(unitId), delay);
                            }
                            // save changes
                            redis.Set(GetUnitTimerJobId(unitId), jobId);
                        }
                        // save changes
                        redis.UpdateUnitShift(unitId, unitShift);
                        logger.LogInformation($"Unit Meta Updated [unit: {unitId}]");
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, $"Update unit meta failed [unitId: {unitId}]. ");
                        var tzNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);
                        logger.LogError($"Current local time: {tzNow:T}");
                        logger.LogError($"Section list: {unitShift.Sections.Select(x => System.Text.Json.JsonSerializer.Serialize(x)).Aggregate((a, b) => a + ", " + b)}");
                        throw;
                    }
                }
            }
        }

        public void ClearHistory()
        {
            using var scope = Service.CreateScope();
            var globalConfig = scope.ServiceProvider.GetRequiredService<IWritableOptions<GlobalSetting>>();
            var shiftService = scope.ServiceProvider.GetRequiredService<IShiftService>();
            try
            {
                var setting = globalConfig.Value.ShiftHistory;
                var enabled = setting.Enabled;
                var limit = setting.Limit;

                DateOnly? targetLimit = null;

                if (enabled)
                {
                    try
                    {
                        var n = int.Parse(limit[0..^1]);
                        char type = limit[^1];
                        switch (type)
                        {
                            case 'Y':
                                targetLimit = DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(DateTime.UtcNow, tz)).AddYears(-n);
                                break;
                            case 'M':
                                targetLimit = DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(DateTime.UtcNow, tz)).AddMonths(-n);
                                break;
                            default:
                                break;
                        }
                    }
                    catch (Exception e)
                    {
                        logger.LogError(e, "Error in process of clear shift history parsing. (use default limit: no limit)");
                        return;
                    }
                }
                shiftService.ClearShiftHistory(targetLimit);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Clear shift history error");
            }
        }
    }
}
