using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Models.Enums;
using Wasenshi.HemoDialysisPro.Services.BusinessLogic;
using Wasenshi.HemoDialysisPro.Services.Interfaces;
using Wasenshi.HemoDialysisPro.Utils;

namespace Wasenshi.HemoDialysisPro.Services
{
    public partial class RecordService : IRecordService
    {
        // ================================ Execution ==============================================


        public IEnumerable<ExecutionRecord> GetExecutionRecordsByHemoId(Guid hemoId)
        {
            return executionRecordRepo.GetAll()
                .Where(x => x.HemodialysisId == hemoId)
                .OrderByDescending(x => x.Timestamp)
                .ToList();
        }

        public ExecutionRecord GetExecutionRecord(Guid id, ExecutionType? type)
        {
            return type switch
            {
                ExecutionType.Medicine => executionRecordRepo.GetMedicineRecords().FirstOrDefault(x => x.Id == id),
                ExecutionType.NSSFlush => executionRecordRepo.Get(id),
                _ => executionRecordRepo.Get(id)

            };
        }

        public ExecutionRecord CreateExecutionRecord(ExecutionRecord record)
        {
            executionRecordRepo.Insert(record);
            executionRecordRepo.Complete();

            return record;
        }

        public MedicineResult CreateMedicineRecords(Guid hemoId, IEnumerable<Guid> medPrescriptions, TimeZoneInfo timeZone = null, bool isSystemUpdate = true)
        {
            return CreateMedicineRecords(new HemodialysisRecord { Id = hemoId, IsSystemUpdate = isSystemUpdate }, medPrescriptions, timeZone);
        }

        public MedicineResult CreateMedicineRecords(HemodialysisRecord hemosheet, IEnumerable<Guid> medPrescriptions, TimeZoneInfo timeZone = null)
        {
            // Get Med Prescriptions info
            var medPresList = medicinePrescriptionRepo.GetAll(false)
                .Where(x => medPrescriptions.Contains(x.Id))
                .ToList();

            MedicineResult result = new MedicineResult();
            if (medPresList.Select(x => x.PatientId).Distinct().Count() > 1)
            {
                result.AddError(Guid.Empty, "Cross patients!");
                return result;
            }

            foreach (var prescription in medPresList)
            {
                if (!medProcessor.CheckAvailablity(prescription, out string reason, timeZone))
                {
                    result.AddError(prescription.Id, reason);
                }
            }

            // Short-circuit and return if errors
            if (!result.IsSuccess)
            {
                return result;
            }

            var now = DateTime.UtcNow;
            var hemoId = hemosheet.Id;
            var isSystemUpdate = hemosheet.IsSystemUpdate;
            List<MedicineRecord> records = new List<MedicineRecord>();
            foreach (var prescriptionId in medPrescriptions)
            {
                var newRecord = new MedicineRecord
                {
                    HemodialysisId = hemoId,
                    PrescriptionId = prescriptionId,
                    Timestamp = now,
                    IsSystemUpdate = isSystemUpdate,
                };
                records.Add(newRecord);
                executionRecordRepo.Insert(newRecord);
            }

            executionRecordRepo.Complete();
            result.Records = records;

            return result;
        }

        public bool UpdateExecutionRecord(ExecutionRecord record)
        {
            var old = executionRecordRepo.Get(record.Id);
            if (old == null)
            {
                throw new AppException("NOT_FOUND", "Execution record doesn't exist.");
            }

            // Safe guard, cannot edit hemoId
            if (old.HemodialysisId != record.HemodialysisId)
            {
                throw new InvalidOperationException("Cannot edit hemosheet id");
            }

            executionRecordRepo.Update(record);

            return executionRecordRepo.Complete() > 0;
        }

        public bool DeleteExecutionRecord(Guid id, ExecutionType type = ExecutionType.Medicine)
        {
            switch (type)
            {
                case ExecutionType.Medicine:
                    executionRecordRepo.Delete(new MedicineRecord { Id = id });
                    break;
                default:
                    return false;
            }

            return executionRecordRepo.Complete() > 0;
        }

        public bool CheckAnyUnexecutedRecord(Guid hemoId)
        {
            return executionRecordRepo.GetAll(false).Where(x => x.HemodialysisId == hemoId && !x.IsExecuted).Any();
        }

        public bool ClaimExecutionRecord(Guid id, Guid userId)
        {
            var record = executionRecordRepo.Get(id);
            if (record == null)
            {
                return false;
            }
            if (record.CreatedBy != Guid.Empty)
            {
                return false;
            }

            record.CreatedBy = userId;
            executionRecordRepo.Update(record);

            return executionRecordRepo.Complete() > 0;
        }
    }
}
