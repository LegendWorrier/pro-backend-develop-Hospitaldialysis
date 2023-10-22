using AutoMapper;
using Hangfire;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ServiceStack.Messaging;
using ServiceStack.Redis;
using System;
using System.Linq;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Services.Interfaces;
using Wasenshi.HemoDialysisPro.Share;
using Wasenshi.HemoDialysisPro.Share.RPCs;
using Wasenshi.HemoDialysisPro.Models.TimezoneUtil;

namespace Wasenshi.HemoDialysisPro.Jobs
{
    public class HemosheetJob
    {
        private TimeZoneInfo tz;
        public IServiceProvider Services { get; }

        private static readonly TimeSpan threshold = TimeSpan.FromSeconds(10); // time frame to prevent double record when auto filling
        private readonly IConfiguration config;
        private readonly ILogger<HemosheetJob> logger;

        public HemosheetJob(
            IConfiguration config,
            IServiceProvider services,
            ILogger<HemosheetJob> logger)
        {
            Services = services;
            tz = TimezoneUtils.GetTimeZone(config.GetValue<string>("TIMEZONE"));
            this.config = config;
            this.logger = logger;
        }

        public void AutoFillRecord(Guid id, TimeSpan interval)
        {
            logger.LogInformation($"[TASK] Auto fill record for {id}...");
            using var scope = Services.CreateScope();
            var redis = scope.ServiceProvider.GetRequiredService<IRedisClient>();
            var message = scope.ServiceProvider.GetRequiredService<IMessageQueueClient>();
            var hemoService = scope.ServiceProvider.GetRequiredService<IHemoService>();
            var recordService = scope.ServiceProvider.GetRequiredService<IRecordService>();
            var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();

            bool stopScheduleFlag = false;
            try
            {
                var hemosheet = hemoService.GetHemodialysisRecord(id);
                if (hemosheet == null || hemosheet.CompletedTime.HasValue)
                {
                    if (hemosheet == null)
                    {
                        logger.LogWarning($"Cannot find hemosheet for auto fill record. [hemoId: {id}]");
                    }
                    else
                    {
                        logger.LogWarning($"Auto fill timer is still running after a hemosheet has been completed. [hemoId: {id}]");
                    }

                    stopScheduleFlag = true;
                    logger.LogInformation("Cancelled job for auto fill.");
                    redis.Remove(Common.GetAutofillJobId(id));

                    return;
                }

                // reuse recently pushed data from machine first
                DialysisRecord record = recordService.FindLatestRecordFromMachine(id, threshold);
                string lastRecordTimestamp = redis.GetValueFromHash(Common.GetHemosheetKey(id), Common.LAST_RECORD_SAVE_TIME);
                if (!string.IsNullOrEmpty(lastRecordTimestamp) && (DateTime.UtcNow - new DateTime(long.Parse(lastRecordTimestamp))) <= threshold)
                {
                    logger.LogInformation($"Skip auto fill, (already have recent record)");
                    return;
                }
                // If no data recently pushed, then manually get current data from machine
                if (record == null)
                {
                    var monitorPool = redis.GetMonitorPool();
                    var bedBox = monitorPool.GetBedByPatientId(hemosheet.PatientId);
                    if (bedBox == null)
                    {
                        logger.LogWarning($"No connection to corresponding HemoBox with the patient [{hemosheet.PatientId}], skip auto fill record for Hemosheet [{id}].");
                        return;
                    }

                    var unique = "mq:dd:" + Guid.NewGuid().ToString("N");
                    var req = Message.Create(new GetDialysisData { ConnectionId = bedBox.ConnectionId });
                    req.ReplyTo = unique;
                    message.Publish<GetDialysisData>(req);

                    var response = message.Get<GetDialysisDataResponse>(unique, TimeSpan.FromSeconds(10)); // timeout here should be greater than timeout in CallWithResponse() function.
                    var manual = response?.GetBody()?.Data;

                    if (manual == null)
                    {
                        logger.LogWarning($"Couldn't get data from machine for patient [{hemosheet.PatientId}], skip auto fill record for hemosheet [{id}].");
                        return;
                    }

                    // copy
                    record = mapper.Map<DialysisRecord>(manual);
                }

                // auto fill record to hemosheet
                record.Id = Guid.Empty;
                record.HemodialysisId = id;
                record.IsSystemUpdate = true;
                record.IsFromMachine = false;
                recordService.CreateDialysisRecord(record);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Auto fill error");
                throw;
            }
            finally
            {
                if (!stopScheduleFlag)
                {
                    // schedule next interval recursively
                    var jobId = BackgroundJob.Schedule<HemosheetJob>(x => x.AutoFillRecord(id, interval), interval);
                    redis.Set(Common.GetAutofillJobId(id), jobId);
                }
            }
        }

        public void StopAutoFill(Guid hemoId)
        {
            _StopAutoFill(hemoId);
        }

        private void _StopAutoFill(Guid hemoId, IServiceScope scope = null)
        {
            using var localScope = Services.CreateScope();
            if (scope == null)
            {
                scope = localScope;
            }
            var redis = scope.ServiceProvider.GetRequiredService<IRedisClient>();
            var lastJobId = redis.Get<string>(Common.GetAutofillJobId(hemoId));
            if (!string.IsNullOrEmpty(lastJobId))
            {
                // Stop and delete the job then clear cache for job key in redis
                BackgroundJob.Delete(lastJobId);
                redis.Remove(Common.GetAutofillJobId(hemoId));
            }
        }


        // ======================== Auto Complete =======================

        public void CompleteHemosheet(Guid hemoId, string patientId)
        {
            using (var scope = Services.CreateScope())
            {
                var hemoService = scope.ServiceProvider.GetRequiredService<IHemoService>();
                hemoService.CompleteHemodialysisRecord(hemoId, new HemodialysisRecord { IsSystemUpdate = true });

                _StopAutoFill(hemoId, scope);

                // untrack the hemosheet
                var redis = scope.ServiceProvider.GetRequiredService<IRedisClient>();
                redis.Remove(Common.GetHemosheetKey(hemoId));
                redis.RemoveSession(patientId);

                // unregister self
                redis.Remove(Common.GetAutoCompleteJobId(hemoId));
            }
        }

        // =============== Auto Fill Medicine ================

        public void AutoMedicine(Guid hemoId, string patientId)
        {
            try
            {
                using var scope = Services.CreateScope();
                var medPresService = scope.ServiceProvider.GetRequiredService<IMedicinePrescriptionService>();
                var medList = medPresService.GetMedicinePrescriptionAutoList(patientId, tz);
                if (medList.Any())
                {
                    var recordService = scope.ServiceProvider.GetRequiredService<IRecordService>();
                    recordService.CreateMedicineRecords(hemoId, medList, tz);
                }
            }
            catch (Exception e)
            {
                logger.LogError(e, "Auto med error");
                throw;
            }
        }
    }
}
