using System;
using System.Collections.Generic;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Services.BusinessLogic;
using Wasenshi.HemoDialysisPro.Services.Interfaces.Base;
using Wasenshi.HemoDialysisPro.Models.Enums;

namespace Wasenshi.HemoDialysisPro.Services.Interfaces
{
    public interface IRecordService : IApplicationService
    {
        // ==== Special For Integration ====
        bool CheckDialysisRecordExistByHemoId(Guid HemoId);
        // ==== Special For Auto Fill Record ===
        DialysisRecord FindLatestRecordFromMachine(Guid hemoId, TimeSpan timeWindow);
        bool IsExistLatestRecordOnHemosheet(Guid hemoId, TimeSpan timeWindow);
        // ============= Dialysis Record =============
        IEnumerable<DialysisRecord> GetDialysisRecordsByHemoId(Guid hemoId);
        IEnumerable<DialysisRecord> GetDialysisRecordsUpdateByHemoId(Guid hemoId, DateTimeOffset? lastData, DateTimeOffset? lastMachineData);
        IEnumerable<DialysisRecord> GetMachineUpdateByHemoId(Guid hemoId, DateTimeOffset? lastData = null);
        DialysisRecord GetDialysisRecord(Guid id);
        DialysisRecord CreateDialysisRecord(DialysisRecord record);
        bool UpdateDialysisRecord(DialysisRecord record);
        bool DeleteDialysisRecord(Guid id);
        // ============== Nurse Record ============
        IEnumerable<NurseRecord> GetNurseRecordsByHemoId(Guid hemoId);
        NurseRecord GetNurseRecord(Guid id);
        NurseRecord CreateNurseRecord(NurseRecord record);
        bool UpdateNurseRecord(NurseRecord record);
        bool DeleteNurseRecord(Guid id);
        // ============== Doctor Record ============
        IEnumerable<DoctorRecord> GetDoctorRecordsByHemoId(Guid hemoId);
        DoctorRecord GetDoctorRecord(Guid id);
        DoctorRecord CreateDoctorRecord(DoctorRecord record);
        bool UpdateDoctorRecord(DoctorRecord record);
        bool DeleteDoctorRecord(Guid id);
        // ============== Execution Record ============
        IEnumerable<ExecutionRecord> GetExecutionRecordsByHemoId(Guid hemoId);
        ExecutionRecord GetExecutionRecord(Guid id, ExecutionType? type = null);
        ExecutionRecord CreateExecutionRecord(ExecutionRecord record);

        MedicineResult CreateMedicineRecords(Guid hemoId, IEnumerable<Guid> medPrescriptions,
            TimeZoneInfo timeZone = null, bool isSystemUpdate = true);
        MedicineResult CreateMedicineRecords(HemodialysisRecord hemosheet, IEnumerable<Guid> medPrescriptions,
            TimeZoneInfo timeZone = null);
        bool UpdateExecutionRecord(ExecutionRecord record);
        bool DeleteExecutionRecord(Guid id, ExecutionType type = ExecutionType.Medicine);

        bool CheckAnyUnexecutedRecord(Guid hemoId);
        bool ClaimExecutionRecord(Guid id, Guid userId);

        // ============== Progress Note Record ============
        IEnumerable<ProgressNote> GetProgressNotesByHemoId(Guid hemoId);
        ProgressNote GetProgressNote(Guid id);
        ProgressNote CreateProgressNote(ProgressNote record);
        bool UpdateProgressNote(ProgressNote record);
        bool DeleteProgressNote(Guid id);
        bool SwapProgressNoteOrder(Guid firstId, Guid secondId);
    }
}
