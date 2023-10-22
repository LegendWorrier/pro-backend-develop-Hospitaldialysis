using Microsoft.EntityFrameworkCore;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Models.Entity.Stockable;
using Wasenshi.HemoDialysisPro.Models.MappingModels;

namespace Wasenshi.HemoDialysisPro.Repositories
{
    public interface IApplicationDbContext : IDbContext
    {
        DbSet<Allergy> Allergies { get; set; }
        DbSet<Anticoagulant> Anticoagulants { get; set; }
        DbSet<Admission> Admissions { get; set; }
        DbSet<AdmissionUnderlying> AdmissionUnderlyings { get; set; }
        DbSet<AssessmentItem> AssessmentItems { get; set; }
        DbSet<AssessmentOption> AssessmentOptions { get; set; }
        DbSet<Assessment> Assessments { get; set; }
        DbSet<AssessmentGroup> AssessmentGroups { get; set; }
        DbSet<DialysisRecordAssessmentItem> DialysisRecordAssessmentItems { get; set; }
        DbSet<AVShuntIssueTreatment> AvShuntIssueTreatments { get; set; }
        DbSet<AVShunt> AvShunts { get; set; }
        DbSet<DeathCause> CauseOfDeath { get; set; }
        DbSet<Dialysate> Dialysates { get; set; }
        DbSet<DialysisPrescription> DialysisPrescriptions { get; set; }
        DbSet<DialysisRecord> DialysisRecords { get; set; }
        DbSet<DoctorRecord> DoctorRecords { get; set; }
        DbSet<ExecutionRecord> ExecutionRecords { get; set; }
        DbSet<FileEntry> Files { get; set; }
        DbSet<FlushRecord> FlushRecords { get; set; }
        DbSet<HemodialysisRecord> HemodialysisRecords { get; set; }
        DbSet<HemoNote> HemoNotes { get; set; }
        DbSet<LabExamItem> LabExamItems { get; set; }
        DbSet<LabExam> LabExams { get; set; }
        DbSet<LabOverview> LabOverviews { get; set; }
        DbSet<MedCategory> MedCategories { get; set; }
        DbSet<MedHistoryItem> MedicineHistories { get; set; }
        DbSet<MedicinePrescription> MedicinePrescriptions { get; set; }
        DbSet<MedicineRecord> MedicineRecords { get; set; }
        DbSet<Needle> Needles { get; set; }
        DbSet<NurseRecord> NurseRecords { get; set; }
        DbSet<Patient> Patients { get; set; }
        DbSet<PatientHistoryItem> PatientHistoryItems { get; set; }
        DbSet<PatientHistory> PatientHistories { get; set; }
        DbSet<ProgressNote> ProgressNotes { get; set; }
        DbSet<ScheduleMeta> ScheduleMeta { get; set; }
        DbSet<Schedule> Schedules { get; set; }
        DbSet<ScheduleSection> Sections { get; set; }
        DbSet<SectionSlotPatient> SectionSlotPatients { get; set; }
        DbSet<ShiftIncharge> ShiftIncharges { get; set; }
        DbSet<ShiftMeta> ShiftMeta { get; set; }
        DbSet<ShiftSlot> ShiftSlots { get; set; }
        DbSet<Status> Status { get; set; }
        DbSet<Tag> Tags { get; set; }
        DbSet<TempSection> TempSections { get; set; }
        DbSet<Unit> Units { get; set; }
        DbSet<UserShift> UserShifts { get; set; }
        DbSet<UserUnit> UserUnits { get; set; }
        DbSet<Underlying> Underlyings { get; set; }

        DbSet<Ward> Wards { get; set; }

        DbSet<LabHemosheet> LabHemosheets { get; set; }

        DbSet<MedicalSupply> MedicalSupplies { get; set;}
        DbSet<Equipment> Equipments { get; set; }
        DbSet<Dialyzer> Dialyzers { get; set; }
        DbSet<Medicine> Medicines { get; set; }
    }
}