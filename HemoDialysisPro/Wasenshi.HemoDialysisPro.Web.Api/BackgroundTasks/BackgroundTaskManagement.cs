using Hangfire;
using ServiceStack.Redis;
using System;
using Wasenshi.HemoDialysisPro.Jobs;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Share;

namespace Wasenshi.HemoDialysisPro.Web.Api.BackgroundTasks
{
    public static class BackgroundTaskManagement
    {
        // ============ Pending Section Update =============
        public static void QueuePendingSectionUpdate(this IBackgroundJobClient bgJob, PendingSectionUpdate info, IRedisClient redis)
        {
            bgJob.ClearPendingSectionUpdate(info.UnitId, redis);

            int unitId = info.UnitId;
            string jobId = bgJob.Schedule<ScheduleManageJob>(x => x.ApplySectionUpdate(unitId), info.TargetDate);

            redis.Set(Common.GetPendingSectionJobId(info.UnitId), jobId);
        }

        public static void ClearPendingSectionUpdate(this IBackgroundJobClient bgJob, int unitId, IRedisClient redis)
        {
            var lastJobId = redis.Get<string>(Common.GetPendingSectionJobId(unitId));
            if (!string.IsNullOrEmpty(lastJobId))
            {
                bgJob.Delete(lastJobId);
                redis.Remove(Common.GetPendingSectionJobId(unitId));
            }
        }
        // ============ Start next round =============
        public static void StartNextRound(this IBackgroundJobClient bgJob, int unitId)
        {
            bgJob.Enqueue<ShiftManagementJob>(x => x.StartNextRound(unitId));
        }
        // ============ Auto Fill Record =============
        public static void StartAutoFillForHemosheet(this IBackgroundJobClient bgJob, Guid hemoId, TimeSpan interval)
        {
            bgJob.StopAutoFill(hemoId);
            bgJob.Enqueue<HemosheetJob>(x => x.AutoFillRecord(hemoId, interval));
        }

        public static void StopAutoFill(this IBackgroundJobClient bgJob, Guid hemoId)
        {
            bgJob.Enqueue<HemosheetJob>(x => x.StopAutoFill(hemoId));
        }

        // ============ Auto Complete =============
        public static void QueueAutoComplete(this IBackgroundJobClient bgJob, HemosheetCompleteTask info, IRedisClient redis)
        {
            bgJob.ClearAutoComplete(info.HemoId, redis);

            string jobId = bgJob.Schedule<HemosheetJob>(x => x.CompleteHemosheet(info.HemoId, info.PatientId), info.TargetDateTime);

            redis.Set(Common.GetAutoCompleteJobId(info.HemoId), jobId);
        }

        public static void ClearAutoComplete(this IBackgroundJobClient bgJob, Guid hemoId, IRedisClient redis)
        {
            var lastJobId = redis.Get<string>(Common.GetAutoCompleteJobId(hemoId));
            if (!string.IsNullOrEmpty(lastJobId))
            {
                bgJob.Delete(lastJobId);
                redis.Remove(Common.GetAutoCompleteJobId(hemoId));
            }
        }

        // =============== Auto Medicine ====================
        public static void AutoMedicine(this IBackgroundJobClient bgJob, HemodialysisRecord hemosheet)
        {
            bgJob.Enqueue<HemosheetJob>(x => x.AutoMedicine(hemosheet.Id, hemosheet.PatientId));
        }
    }

    public struct PendingSectionUpdate
    {
        public DateTimeOffset TargetDate { get; set; }
        public int UnitId { get; set; }
    }

    public struct HemosheetCompleteTask
    {
        public Guid HemoId { get; set; }
        public string PatientId { get; set; }
        public DateTimeOffset TargetDateTime { get; set; }
    }
}
