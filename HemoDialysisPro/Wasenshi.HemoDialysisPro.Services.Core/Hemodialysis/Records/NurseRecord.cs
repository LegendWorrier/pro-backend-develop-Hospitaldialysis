using System;
using System.Collections.Generic;
using System.Linq;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Services.Interfaces;

namespace Wasenshi.HemoDialysisPro.Services
{
    public partial class RecordService : IRecordService
    {
        // ================================ Nurse ==============================================


        public IEnumerable<NurseRecord> GetNurseRecordsByHemoId(Guid hemoId)
        {
            return nurseRecordRepo.GetAll()
                .Where(x => x.HemodialysisId == hemoId)
                .OrderByDescending(x => x.Timestamp)
                .ToList();
        }

        public NurseRecord GetNurseRecord(Guid id)
        {
            var result = nurseRecordRepo.Get(id);
            return result;
        }

        public NurseRecord CreateNurseRecord(NurseRecord record)
        {
            nurseRecordRepo.Insert(record);
            nurseRecordRepo.Complete();

            return record;
        }

        public bool UpdateNurseRecord(NurseRecord record)
        {
            var old = nurseRecordRepo.Get(record.Id);

            // Safe guard, cannot edit hemoId
            if (old.HemodialysisId != record.HemodialysisId)
            {
                throw new InvalidOperationException("Cannot edit hemosheet id");
            }

            nurseRecordRepo.Update(record);

            return nurseRecordRepo.Complete() > 0;
        }

        public bool DeleteNurseRecord(Guid id)
        {
            nurseRecordRepo.Delete(new NurseRecord { Id = id });

            return nurseRecordRepo.Complete() > 0;
        }
    }
}
