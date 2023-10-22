using Microsoft.EntityFrameworkCore;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Models.Entity.Stockable;
using Wasenshi.HemoDialysisPro.Models.MappingModels;
using Wasenshi.HemoDialysisPro.Repositories;
using Wasenshi.HemoDialysisPro.Repository.Core;

namespace Wasenshi.HemoDialysisPro.JobsServer
{
    public class ApplicationDbContextJobServer : DbContext, IApplicationDbContext
    {
        public DbSet<UserImp> Users { get; set; }
        public DbSet<RoleImp> Roles { get; set; }
        public DbSet<UserRoleImp> UserRoles { get; set; }

        public DbSet<Unit> Units { get; set; }
        public DbSet<UserUnit> UserUnits { get; set; }

        public DbSet<ScheduleSection> Sections { get; set; }
        public DbSet<TempSection> TempSections { get; set; } // Temporary data for scheduled updating
        public DbSet<SectionSlotPatient> SectionSlotPatients { get; set; }
        public DbSet<Schedule> Schedules { get; set; }

        public DbSet<ScheduleMeta> ScheduleMeta { get; set; }
        public DbSet<ShiftMeta> ShiftMeta { get; set; }
        public DbSet<ShiftSlot> ShiftSlots { get; set; }
        public DbSet<UserShift> UserShifts { get; set; } // for whole month schedule
        public DbSet<ShiftIncharge> ShiftIncharges { get; set; }


        public DbSet<FileEntry> Files { get; set; }

        public DbSet<Patient> Patients { get; set; }
        public DbSet<PatientHistoryItem> PatientHistoryItems { get; set; }
        public DbSet<PatientHistory> PatientHistories { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<Allergy> Allergies { get; set; }
        public DbSet<MedHistoryItem> MedicineHistories { get; set; }
        public DbSet<Admission> Admissions { get; set; }
        public DbSet<AdmissionUnderlying> AdmissionUnderlyings { get; set; }

        public DbSet<AVShunt> AvShunts { get; set; }
        public DbSet<AVShuntIssueTreatment> AvShuntIssueTreatments { get; set; }

        public DbSet<HemodialysisRecord> HemodialysisRecords { get; set; }
        public DbSet<HemoNote> HemoNotes { get; set; }
        public DbSet<DialysisPrescription> DialysisPrescriptions { get; set; }
        public DbSet<DialysisRecord> DialysisRecords { get; set; }
        public DbSet<NurseRecord> NurseRecords { get; set; }
        public DbSet<DoctorRecord> DoctorRecords { get; set; }
        public DbSet<MedicinePrescription> MedicinePrescriptions { get; set; }
        public DbSet<MedicineRecord> MedicineRecords { get; set; }
        public DbSet<ProgressNote> ProgressNotes { get; set; }

        public DbSet<ExecutionRecord> ExecutionRecords { get; set; }
        public DbSet<FlushRecord> FlushRecords { get; set; }

        public DbSet<Assessment> Assessments { get; set; }
        public DbSet<AssessmentGroup> AssessmentGroups { get; set; }
        public DbSet<AssessmentOption> AssessmentOptions { get; set; }
        public DbSet<AssessmentItem> AssessmentItems { get; set; }
        public DbSet<DialysisRecordAssessmentItem> DialysisRecordAssessmentItems { get; set; }

        public DbSet<LabExam> LabExams { get; set; }
        public DbSet<LabHemosheet> LabHemosheets { get; set; }

        public DbSet<Medicine> Medicines { get; set; }
        public DbSet<MedCategory> MedCategories { get; set; } // Medicine Category
        public DbSet<Status> Status { get; set; }
        public DbSet<DeathCause> CauseOfDeath { get; set; }
        public DbSet<Dialysate> Dialysates { get; set; }
        public DbSet<Dialyzer> Dialyzers { get; set; }
        public DbSet<Anticoagulant> Anticoagulants { get; set; }
        public DbSet<Needle> Needles { get; set; }
        public DbSet<LabExamItem> LabExamItems { get; set; }
        public DbSet<Underlying> Underlyings { get; set; }
        public DbSet<Ward> Wards { get; set; }

        public DbSet<MedicalSupply> MedicalSupplies { get; set; }
        public DbSet<Equipment> Equipments { get; set; }

        // ================== Views (Query Only) ====================
        public DbSet<LabOverview> LabOverviews { get; set; }

        public ApplicationDbContextJobServer(DbContextOptions<ApplicationDbContextJobServer> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            var modelSetup = new ContextModelSetup<UserImp, RoleImp>();
            modelSetup.SetupModel(builder);

            builder.Entity<UserRoleImp>()
                .ToTable("AspNetUserRoles")
                .HasKey(x => new { x.UserId, x.RoleId });
        }

        // ==================== Common Bases code (Jobs Version) ==============================

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken)
        {
            BeforeSaveChanges();
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken)
        {
            BeforeSaveChanges();
            return base.SaveChangesAsync(cancellationToken);
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            BeforeSaveChanges();
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        public override int SaveChanges()
        {
            BeforeSaveChanges();
            return base.SaveChanges();
        }
        protected virtual void BeforeSaveChanges()
        {
            var entities = ChangeTracker.Entries()
                .Where(x => x.Entity is EntityBase && (x.State == EntityState.Added || x.State == EntityState.Modified));


            foreach (var entity in entities)
            {
                if (!(entity.Entity is EntityBase entityBase)) continue;


                var now = DateTime.UtcNow; // current datetime
                Guid userId = Guid.Empty;

                if (entity.State == EntityState.Added)
                {
                    entityBase.Created = now;
                    entityBase.CreatedBy = userId;
                }

                if (entity.State != EntityState.Unchanged)
                {
                    entityBase.Updated = now;
                    entityBase.UpdatedBy = userId;
                }
            }
        }
    }
}
