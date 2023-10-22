using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Models.Entity.Stockable;
using Wasenshi.HemoDialysisPro.Models.Interfaces;
using Wasenshi.HemoDialysisPro.Models.MappingModels;
using Wasenshi.HemoDialysisPro.Repositories;
using Wasenshi.HemoDialysisPro.Models.Enums;

namespace Wasenshi.HemoDialysisPro.Repository.Core
{
    public class ContextModelSetup<TUser, TRole> where TUser : class, IUser where TRole : class, IRole
    {
        public ContextModelSetup()
        {
        }

        public virtual ModelBuilder SetupModel(ModelBuilder builder)
        {
            // For AspNet Identity Creation
            builder.Entity<TUser>().ToTable("Users").HasKey(x => x.Id);
            builder.Entity<TRole>().ToTable("Roles").HasKey(x => x.Id);

            builder.Entity<ScheduleSection>()
                .HasOne(x => x.Unit)
                .WithMany(x => x.Sections)
                .HasForeignKey(x => x.UnitId)
                .IsRequired();

            builder.Entity<SectionSlotPatient>(x =>
            {
                x.HasKey(x => new { x.PatientId, x.SectionId, x.Slot });
                x.HasOne(x => x.Patient).WithMany().HasForeignKey(x => x.PatientId).IsRequired();
                x.HasOne(x => x.Section).WithMany().HasForeignKey(x => x.SectionId).IsRequired();
            });
            builder.Entity<Schedule>(x =>
            {
                x.HasOne(x => x.Patient).WithMany().HasForeignKey(x => x.PatientId).IsRequired();
                x.HasOne(x => x.Section).WithMany().HasForeignKey(x => x.SectionId).IsRequired();
            });

            builder.Entity<ShiftMeta>()
                .HasOne(x => x.ScheduleMeta).WithMany().HasForeignKey(x => x.ScheduleMetaId).IsRequired();
            builder.Entity<ShiftIncharge>(x =>
            {
                x.HasKey(x => new { x.UnitId, x.Date });
                x.OwnsMany(x => x.Sections, c => c.HasOne(x => x.Section).WithMany().HasForeignKey(x => x.SectionId).IsRequired());
            });

            builder.Entity<ShiftSlot>(x =>
            {
                x.HasOne(x => x.ShiftMeta).WithMany().HasForeignKey(x => x.ShiftMetaId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);
                x.HasIndex(x => new { x.UserId, x.Date }).HasDatabaseName("shift_slot_unique_keys").IsUnique();
            });

            builder.Entity<UserShift>()
                .HasIndex(x => new { x.UserId, x.Month }).HasDatabaseName("user_shift_unique_keys").IsUnique();

            builder.Entity<UserUnit>(x =>
            {
                x.HasKey(x => new { x.UnitId, x.UserId });
                x.HasOne(x => x.Unit).WithMany().HasForeignKey(x => x.UnitId).IsRequired();
            });
            builder.Entity<TUser>()
                .HasMany(x => x.Units).WithOne().HasForeignKey(x => x.UserId).IsRequired();

            builder.Entity<PatientMedicine>(b =>
            {
                b.HasDiscriminator<string>("Type");
                b.HasIndex("Type", "PatientId", "MedicineId").IsUnique().HasDatabaseName("Index_Unique");
                b.HasKey(x => new { x.PatientId, x.MedicineId });
            });

            builder.Entity<Allergy>(x =>
            {
                x.HasIndex(x => x.PatientId).HasDatabaseName("Index_PatientId_Allergy");
                x.HasOne(x => x.Patient).WithMany(x => x.Allergy).HasForeignKey(x => x.PatientId).HasConstraintName("Patient_Allergy").IsRequired();
                x.HasOne(x => x.Medicine).WithMany(x => x.Allergies).HasForeignKey(x => x.MedicineId).HasConstraintName("Medicine_Allergy").IsRequired();
            });

            builder.Entity<MedHistoryItem>()
                .HasOne(x => x.Medicine)
                .WithMany()
                .IsRequired();

            builder.Entity<AdmissionUnderlying>(x =>
            {
                x.HasIndex(x => x.AdmissionId).HasDatabaseName("Index_Admission_Underlying");
                x.HasOne(x => x.Admission).WithMany(x => x.Underlying).HasForeignKey(x => x.AdmissionId).HasConstraintName("Admission_Underlying").IsRequired();
                x.HasOne(x => x.Underlying).WithMany().HasForeignKey(x => x.UnderlyingId).HasConstraintName("Underlying_Underlying").IsRequired();
                x.HasKey(x => new { x.AdmissionId, x.UnderlyingId });
            });

            builder.Entity<Patient>(x =>
            {
                x.HasIndex(x => x.Name).HasDatabaseName("Index_Name");
                x.HasMany(x => x.Tags)
                    .WithOne(x => x.Patient)
                    .HasForeignKey(x => x.PatientId)
                    .IsRequired();
            });

            builder.Entity<HemodialysisRecord>()
                .HasOne(x => x.DialysisPrescription)
                .WithMany(x => x.HemodialysisRecords);

            builder.Entity<HemoNote>(x =>
            {
                x.HasIndex(x => x.HemoId).HasDatabaseName("Index_HemoId").IsUnique();
                x.HasOne(x => x.Hemosheet).WithOne(x => x.Note).HasForeignKey<HemoNote>(x => x.HemoId).IsRequired();
            });

            builder.Entity<Assessment>(c =>
            {
                c.HasMany(x => x.OptionsList)
                .WithOne()
                .HasForeignKey(x => x.AssessmentId)
                .IsRequired();

                c.HasOne(x => x.Group)
                .WithMany()
                .HasForeignKey(x => x.GroupId)
                .IsRequired(false);
            });

            builder.Entity<AssessmentItem>(c =>
            {
                c.HasOne(x => x.Hemosheet)
                    .WithMany()
                    .HasForeignKey(x => x.HemosheetId)
                    .HasConstraintName("Hemosheet")
                    .IsRequired();
                c.HasOne(x => x.Assessment)
                    .WithMany()
                    .HasForeignKey(x => x.AssessmentId)
                    .IsRequired();
            });

            builder.Entity<DialysisRecordAssessmentItem>(c =>
            {
                c.HasOne(x => x.DialysisRecord)
                    .WithMany(x => x.AssessmentItems)
                    .HasForeignKey(x => x.DialysisRecordId)
                    .HasConstraintName("DialysisRecord")
                    .IsRequired();
                c.HasOne(x => x.Assessment)
                    .WithMany()
                    .HasForeignKey(x => x.AssessmentId)
                    .IsRequired();
            });

            builder.Entity<Medicine>()
                .HasOne(x => x.Category)
                .WithMany()
                .HasForeignKey(x => x.CategoryId);

            builder.Entity<ExecutionRecord>(c =>
            {
                c.HasDiscriminator(x => x.Type)
                    .HasValue<MedicineRecord>(ExecutionType.Medicine)
                    .HasValue<FlushRecord>(ExecutionType.NSSFlush);

                c.HasOne(x => x.Hemodialysis).WithMany()
                    .HasForeignKey(x => x.HemodialysisId)
                    .IsRequired();
            });


            builder.Entity<MedicinePrescription>(c =>
            {
                c.HasOne(x => x.Medicine).WithMany()
                    .HasForeignKey(x => x.MedicineId)
                    .IsRequired();

                c.HasMany(x => x.MedicineRecords)
                    .WithOne(x => x.Prescription)
                    .HasForeignKey(x => x.PrescriptionId)
                    .IsRequired();
            });

            builder.Entity<ProgressNote>()
                .HasOne(x => x.Hemodialysis).WithMany()
                .HasForeignKey(x => x.HemodialysisId)
                .IsRequired();

            builder.Entity<LabExamItem>()
                .HasIndex(x => x.Name)
                .IsUnique();

            builder.Entity<LabExam>(c =>
            {
                c.HasOne(x => x.LabItem)
                    .WithMany()
                    .HasForeignKey(x => x.LabItemId)
                    .IsRequired();
                c.HasOne(x => x.Patient)
                    .WithMany()
                    .HasForeignKey(x => x.PatientId)
                    .IsRequired();
            });

            builder.Entity<LabHemosheet>(x =>
            {
                x.HasKey(x => x.LabItemId);
                x.HasOne(x => x.Item).WithOne().HasForeignKey<LabHemosheet>(x => x.LabItemId).IsRequired();
            });

            builder.Entity<LabOverview>(c =>
            {
                c.HasNoKey();
                c.HasOne(x => x.Patient).WithMany().HasForeignKey(x => x.PatientId).IsRequired();
                c.ToView(nameof(IApplicationDbContext.LabOverviews));
            });

            // Stock

            builder.Entity<StockItemBase>(b =>
            {
                b.ToTable("StockItems");
            });
            builder.Entity<MedicalSupplyStock>()
                .ToTable(nameof(MedicalSupplyStock))
                .HasBaseType<StockItemBase>()
                .HasOne(x => x.ItemInfo).WithMany().HasForeignKey(x => x.ItemId)
                .IsRequired();
            builder.Entity<EquipmentStock>()
                .ToTable(nameof(EquipmentStock))
                .HasBaseType<StockItemBase>()
                .HasOne(x => x.ItemInfo).WithMany().HasForeignKey(x => x.ItemId)
                .IsRequired();
            builder.Entity<MedicineStock>()
                .ToTable(nameof(MedicineStock))
                .HasBaseType<StockItemBase>()
                .HasOne(x => x.ItemInfo).WithMany().HasForeignKey(x => x.ItemId)
                .IsRequired();
            builder.Entity<DialyzerStock>()
                .ToTable(nameof(DialyzerStock))
                .HasBaseType<StockItemBase>()
                .HasOne(x => x.ItemInfo).WithMany().HasForeignKey(x => x.ItemId)
                .IsRequired();

            return builder;
        }
    }
}
