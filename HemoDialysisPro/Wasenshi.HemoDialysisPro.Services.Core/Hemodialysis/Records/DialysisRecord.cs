using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Services.Interfaces;

namespace Wasenshi.HemoDialysisPro.Services
{
    public partial class RecordService : IRecordService
    {
        // ================================ Dialysis ==============================================


        public bool CheckDialysisRecordExistByHemoId(Guid HemoId)
        {
            return dialysisRecordRepo.GetAll(false).Any(x => x.HemodialysisId == HemoId && !x.IsFromMachine);
        }
        public DialysisRecord FindLatestRecordFromMachine(Guid hemoId, TimeSpan timeWindow)
        {
            var records = dialysisRecordRepo.GetAll(false).Where(x => x.HemodialysisId == hemoId && x.IsFromMachine);
            if (timeWindow > TimeSpan.Zero)
            {
                var limit = DateTime.UtcNow - timeWindow;
                logger.LogInformation($"find machine record within: {limit}");
                records = records.Where(x => x.Timestamp >= limit);
            }
            return records.OrderByDescending(x => x.Timestamp).FirstOrDefault();
        }

        public bool IsExistLatestRecordOnHemosheet(Guid hemoId, TimeSpan timeWindow)
        {
            var records = dialysisRecordRepo.GetAll(false).Where(x => x.HemodialysisId == hemoId && !x.IsFromMachine);
            if (timeWindow > TimeSpan.Zero)
            {
                var limit = DateTime.UtcNow - timeWindow;
                logger.LogInformation($"find hemosheet record within: {limit}");
                records = records.Where(x => x.Timestamp >= limit);
            }
            return records.Any();
        }

        public IEnumerable<DialysisRecord> GetDialysisRecordsByHemoId(Guid hemoId)
        {
            return dialysisRecordRepo.GetAll()
                .Where(x => x.HemodialysisId == hemoId)
                .OrderBy(x => x.IsFromMachine)
                .ThenByDescending(x => x.Timestamp)
                .ToList();
        }

        public IEnumerable<DialysisRecord> GetDialysisRecordsUpdateByHemoId(Guid hemoId, DateTimeOffset? lastData, DateTimeOffset? lastMachineData)
        {
            DateTime cutoff = lastData.HasValue ? lastData.Value.UtcDateTime : DateTime.MinValue;
            DateTime machineCutoff = lastMachineData.HasValue ? lastMachineData.Value.UtcDateTime : DateTime.MinValue;
            return dialysisRecordRepo.GetAll()
                .Where(x => x.HemodialysisId == hemoId)
                .Where(x => (!x.IsFromMachine && x.Timestamp > cutoff) || (x.IsFromMachine && x.Timestamp > machineCutoff))
                .OrderBy(x => x.IsFromMachine)
                .ThenByDescending(x => x.Timestamp)
                .ToList();
        }

        public IEnumerable<DialysisRecord> GetMachineUpdateByHemoId(Guid hemoId, DateTimeOffset? lastData = null)
        {
            var sql = dialysisRecordRepo.Find(x => x.HemodialysisId == hemoId && x.IsFromMachine, false);
            if (lastData.HasValue)
            {
                DateTime cutoff = lastData.Value.UtcDateTime;
                sql = sql.Where(x => x.Timestamp > cutoff);
            }

            return sql.OrderByDescending(x => x.Timestamp).ToList();
        }

        public DialysisRecord GetDialysisRecord(Guid id)
        {
            var result = dialysisRecordRepo.Get(id);
            return result;
        }

        public DialysisRecord CreateDialysisRecord(DialysisRecord record)
        {
            dialysisRecordRepo.Insert(record);
            dialysisRecordRepo.Complete();

            ServiceEvents.DispatchCreate(record);

            return record;
        }

        public bool UpdateDialysisRecord(DialysisRecord record)
        {
            var old = dialysisRecordRepo.Get(record.Id);

            // Safe guard, cannot edit hemoId
            if (old.HemodialysisId != record.HemodialysisId)
            {
                throw new InvalidOperationException("Cannot edit hemosheet id");
            }

            dialysisRecordRepo.SyncCollection<DialysisRecordAssessmentItem, Guid>(record, x => x.AssessmentItems);
            dialysisRecordRepo.Update(record);

            return dialysisRecordRepo.Complete() > 0;
        }

        public bool DeleteDialysisRecord(Guid id)
        {
            dialysisRecordRepo.Delete(new DialysisRecord { Id = id });

            return dialysisRecordRepo.Complete() > 0;
        }
    }
}
