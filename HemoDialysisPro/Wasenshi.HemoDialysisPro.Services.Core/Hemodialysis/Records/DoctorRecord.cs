using System;
using System.Collections.Generic;
using System.Linq;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Services.Interfaces;

namespace Wasenshi.HemoDialysisPro.Services
{
    public partial class RecordService : IRecordService
    {
        // ================================ Doctor ==============================================


        public IEnumerable<DoctorRecord> GetDoctorRecordsByHemoId(Guid hemoId)
        {
            return doctorRecordRepo.GetAll()
                .Where(x => x.HemodialysisId == hemoId)
                .OrderByDescending(x => x.Timestamp)
                .ToList();
        }

        public DoctorRecord GetDoctorRecord(Guid id)
        {
            var result = doctorRecordRepo.Get(id);
            return result;
        }

        public DoctorRecord CreateDoctorRecord(DoctorRecord record)
        {
            doctorRecordRepo.Insert(record);
            doctorRecordRepo.Complete();

            return record;
        }

        public bool UpdateDoctorRecord(DoctorRecord record)
        {
            var old = doctorRecordRepo.Get(record.Id);

            // Safe guard, cannot edit hemoId
            if (old.HemodialysisId != record.HemodialysisId)
            {
                throw new InvalidOperationException("Cannot edit hemosheet id");
            }

            doctorRecordRepo.Update(record);

            return doctorRecordRepo.Complete() > 0;
        }

        public bool DeleteDoctorRecord(Guid id)
        {
            doctorRecordRepo.Delete(new DoctorRecord { Id = id });

            return doctorRecordRepo.Complete() > 0;
        }
    }
}
