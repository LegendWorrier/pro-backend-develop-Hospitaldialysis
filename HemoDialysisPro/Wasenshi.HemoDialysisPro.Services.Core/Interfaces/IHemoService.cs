using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Services.Interfaces.Base;

namespace Wasenshi.HemoDialysisPro.Services.Interfaces
{
    public interface IHemoService : IApplicationService
    {
        Page<HemoRecordResult> GetAllHemodialysisRecords(int page = 1, int limit = 25,
            Action<IOrderer<HemoRecordResult>> orderBy = null,
            Expression<Func<HemoRecordResult, bool>> condition = null);

        Page<HemodialysisRecord> GetAllHemodialysisRecordsWithNote(int page = 1, int limit = 25,
           Action<IOrderer<HemodialysisRecord>> orderBy = null,
           Expression<Func<HemodialysisRecord, bool>> condition = null);

        int CountAll(Expression<Func<HemoRecordResult, bool>> whereCondition = null);
        HemodialysisRecord GetHemodialysisRecordByPatientId(string patientId);
        bool CheckDialysisPrescriptionExists(string patientId);
        HemodialysisRecord GetPreviousHemosheet(HemodialysisRecord hemosheet);
        IEnumerable<DialysisPrescription> GetDialysisPrescriptionsByPatientId(string patientId);
        Page<HemodialysisRecord> GetHemodialysisRecordsByPatientId(string patientId, int page = 1, int limit = 25, Expression<Func<HemodialysisRecord, bool>> whereCondition = null);
        DialysisPrescription GetDialysisPrescription(Guid prescriptionId);
        HemodialysisRecord GetHemodialysisRecord(Guid recordId);
        bool DeleteHemosheet(Guid hemoId);

        DialysisPrescription CreatePrescription(DialysisPrescription prescription);
        bool EditPrescription(DialysisPrescription prescription);
        bool DeletePrescription(Guid id);

        HemodialysisRecord CreateHemodialysisRecord(HemodialysisRecord hemoRecord, ScheduleSection shiftSection = null);
        bool EditHemodialysisRecord(HemodialysisRecord hemoRecord, bool markCompleted = false);
        bool CompleteHemodialysisRecord(Guid id, HemodialysisRecord update = null);
        bool ChangeCompleteTime(Guid hemoId, DateTimeOffset newTime);
        bool UpdateDoctorConsent(Guid recordId, bool consent = true);
        bool ClaimHemosheet(Guid recordId, Guid userId);

        bool UpdateNurseInShift(Guid hemoId, IEnumerable<Guid> nursesList);

        HemoNote UpdateHemoNote(HemoNote hemoNote);
    }
}
