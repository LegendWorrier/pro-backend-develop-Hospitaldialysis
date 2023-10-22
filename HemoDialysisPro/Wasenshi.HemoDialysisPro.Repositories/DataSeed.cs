using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using Microsoft.EntityFrameworkCore;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Models.Entity.Stockable;
using Wasenshi.HemoDialysisPro.Models.Infrastructor;
using Wasenshi.HemoDialysisPro.Models.Enums;
using System.Collections.Generic;

namespace Wasenshi.HemoDialysisPro.Repositories
{
    public static class DataSeed
    {
        public const string AdminUsername = "rootadmin";
        public const string AdminPassword = "@dmin1234";

        public const string SeedFolder = "Seeds";

        public static void Seed(this ModelBuilder builder)
        {
            InitRoleAndRootAdmin(builder);
            InitMasterData(builder);
            InitAssessmentData(builder);
        }

        private static void InitRoleAndRootAdmin(ModelBuilder builder)
        {
            // Add roles
            builder.Entity<Role>()
                .HasData(
                    new
                    {
                        Id = new Guid("2bade702-40e6-4df4-890d-7f0c67d50207"),
                        ConcurrencyStamp = "296e852f-fbf5-4871-a12f-7f039db07929",
                        Name = "SuperAdministrator",
                        NormalizedName = "SUPERADMINISTRATOR"
                    },
                    new
                    {
                        Id = new Guid("e958724f-6db8-4434-a804-f0277f02306a"),
                        ConcurrencyStamp = "faf97553-37bf-4eb7-bd25-721f0b51de9b",
                        Name = "Administrator",
                        NormalizedName = "ADMINISTRATOR"
                    },
                    new
                    {
                        Id = new Guid("a7c93a18-5c50-4e28-b02d-3b50cc3e17f1"),
                        ConcurrencyStamp = "44e7df22-327d-4e8d-9f87-2f59bc6b1cdc",
                        Name = "Doctor",
                        NormalizedName = "DOCTOR"
                    },
                    new
                    {
                        Id = new Guid("baad21b1-6d08-4823-8cf3-0776a6266488"),
                        ConcurrencyStamp = "eab1f073-836a-42ad-9897-463792207f7d",
                        Name = "HeadNurse",
                        NormalizedName = "HEADNURSE"
                    },
                    new
                    {
                        Id = new Guid("f903d36f-2531-4916-86bb-7b051aac7029"),
                        ConcurrencyStamp = "1f165522-607b-4ffa-9885-a8a341fb01d0",
                        Name = "Nurse",
                        NormalizedName = "NURSE"
                    },
                    new
                    {
                        Id = new Guid("9453a296-a401-480b-bf81-bca7ab0f72a7"),
                        ConcurrencyStamp = "eeeef629-5a29-4607-98ef-9fa4a68ac365",
                        Name = "PN",
                        NormalizedName = "PN"
                    });
            // Add root admin
            var rootadmin = new
            {
                Id = new Guid("866dabc4-6501-44d2-a0e5-65da9c45a46e"),
                AccessFailedCount = 0,
                ConcurrencyStamp = "aadf088b-cf32-4817-8cfc-2c3ae4615fcb",
                EmailConfirmed = true,
                FirstName = "root",
                LastName = "admin",
                LockoutEnabled = false,
                IsPartTime = false,
                NormalizedUserName = "ROOTADMIN",
                PasswordHash = "AQAAAAEAACcQAAAAECMCAfv9MIbmyL/FfP+0ZaKk+COwT3IfxeviPM4NRZbhEvX+N0oAYWZsG0lPuKLasA==",
                PhoneNumberConfirmed = true,
                SecurityStamp = "244fb073-9c47-4d58-ad27-5bd36aef059a",
                TwoFactorEnabled = false,
                UserName = "rootadmin"
            };

            builder.Entity<User>()
                .HasData(rootadmin);
            // Assign all roles to root admin
            builder.Entity<UserRole>()
                .HasData(
                    new
                    {
                        UserId = new Guid("866dabc4-6501-44d2-a0e5-65da9c45a46e"),
                        RoleId = new Guid("2bade702-40e6-4df4-890d-7f0c67d50207")
                    },
                    new
                    {
                        UserId = new Guid("866dabc4-6501-44d2-a0e5-65da9c45a46e"),
                        RoleId = new Guid("e958724f-6db8-4434-a804-f0277f02306a")
                    },
                    new
                    {
                        UserId = new Guid("866dabc4-6501-44d2-a0e5-65da9c45a46e"),
                        RoleId = new Guid("a7c93a18-5c50-4e28-b02d-3b50cc3e17f1")
                    },
                    new
                    {
                        UserId = new Guid("866dabc4-6501-44d2-a0e5-65da9c45a46e"),
                        RoleId = new Guid("baad21b1-6d08-4823-8cf3-0776a6266488")
                    },
                    new
                    {
                        UserId = new Guid("866dabc4-6501-44d2-a0e5-65da9c45a46e"),
                        RoleId = new Guid("f903d36f-2531-4916-86bb-7b051aac7029")
                    },
                    new
                    {
                        UserId = new Guid("866dabc4-6501-44d2-a0e5-65da9c45a46e"),
                        RoleId = new Guid("9453a296-a401-480b-bf81-bca7ab0f72a7")
                    });
        }

        private static void InitMasterData(ModelBuilder builder)
        {
            builder.Entity<Unit>()
                .HasData(
                    new Unit
                    {
                        Id = -1,
                        Name = "Unit 1"
                    }
                );

            builder.Entity<Status>()
                .HasData(
                    new Status
                    {
                        Id = -1,
                        Name = "Hospital Transferred",
                        Category = StatusCategories.Transferred
                    },
                    new Status
                    {
                        Id = -2,
                        Name = "Deceased",
                        Category = StatusCategories.Deceased
                    },
                    new Status
                    {
                        Id = -3,
                        Name = "Full Recovery",
                        Category = StatusCategories.Others
                    }
                );

            builder.Entity<DeathCause>()
                .HasData(
                    new DeathCause
                    {
                        Id = -1,
                        Name = "Heart Failure"
                    },
                    new DeathCause
                    {
                        Id = -2,
                        Name = "Hypertension"
                    }
                );

            builder.Entity<Medicine>()
                .HasData(
                    new Medicine
                    {
                        Id = -1,
                        Name = "Heparin"
                    },
                    new Medicine
                    {
                        Id = -2,
                        Name = "Diovan"
                    },
                    new Medicine
                    {
                        Id = -3,
                        Name = "Buscopan"
                    },
                    new Medicine
                    {
                        Id = -4,
                        Name = "Aminoven"
                    },
                    new Medicine
                    {
                        Id = -5,
                        Name = "NGT"
                    }
                );

            builder.Entity<MedCategory>()
                .HasData(
                    new MedCategory
                    {
                        Id = -1,
                        Name = "Oral"
                    },
                    new MedCategory
                    {
                        Id = -2,
                        Name = "Injection"
                    },
                    new MedCategory
                    {
                        Id = -3,
                        Name = "Insulin"
                    },
                    new MedCategory
                    {
                        Id = -4,
                        Name = "ST and Injection"
                    }
                );

            builder.Entity<Needle>()
                .HasData(
                    new Needle
                    {
                        Id = -1,
                        Number = 15
                    },
                    new Needle
                    {
                        Id = -2,
                        Number = 16
                    },
                    new Needle
                    {
                        Id = -3,
                        Number = 17
                    }
                );

            builder.Entity<Anticoagulant>()
                .HasData(
                    new Anticoagulant
                    {
                        Id = -1,
                        Name = "Heparin"
                    },
                    new Anticoagulant
                    {
                        Id = -2,
                        Name = "Clexane"
                    }
                );

            builder.Entity<Dialyzer>()
                .HasData(
                    new Dialyzer
                    {
                        Id = -1,
                        Name = "FDX-21",
                        BrandName = "Nikkiso",
                        Flux = DialyzerType.HighFlux,
                        Membrane = MembraneType.Synthetic_Cellulose,
                        SurfaceArea = 2.1f
                    },
                    new Dialyzer
                    {
                        Id = -2,
                        Name = "FDY-21",
                        BrandName = "Nikkiso",
                        Flux = DialyzerType.HighFlux,
                        Membrane = MembraneType.Synthetic_Cellulose,
                        SurfaceArea = 2.1f
                    }
                );

            builder.Entity<LabExamItem>()
                .HasData(
                    new LabExamItem
                    {
                        Id = -1,
                        Name = "BUN",
                        Unit = "mg/dL",
                        LowerLimit = 6,
                        UpperLimit = 25,
                        IsSystemBound = true,
                        Bound = SpecialLabItem.BUN
                    },
                    new LabExamItem
                    {
                        Id = -2,
                        Name = "Kt/V",
                        Unit = "",
                        IsSystemBound = true,
                        Bound = SpecialLabItem.KtV,
                        IsCalculated = true
                    },
                    new LabExamItem
                    {
                        Id = -3,
                        Name = "URR",
                        Unit = "%",
                        IsSystemBound = true,
                        Bound = SpecialLabItem.URR,
                        IsCalculated = true
                    }
                );

            builder.Entity<PatientHistoryItem>(c =>
            {
                c.HasData(
                    new PatientHistoryItem
                    {
                        Id = -1,
                        Order = 1,
                        Name = "height",
                        DisplayName = "Height",
                        TRT = TRTMappingPatient.Height,
                        IsNumber = true,
                    },
                    new PatientHistoryItem
                    {
                        Id = -2,
                        Order = 2,
                        Name = "weight",
                        DisplayName = "Weight",
                        TRT = TRTMappingPatient.Weight,
                        IsNumber = true,
                    },
                    new PatientHistoryItem
                    {
                        Id = -3,
                        Order = 3,
                        Name = "marriage",
                        DisplayName = "Marriage Status",
                        TRT = TRTMappingPatient.Marriage,
                        AllowOther = false
                    },
                    new PatientHistoryItem
                    {
                        Id = -4,
                        Order = 4,
                        Name = "education",
                        DisplayName = "Education",
                        TRT = TRTMappingPatient.Education,
                        AllowOther = true
                    },
                    new PatientHistoryItem
                    {
                        Id = -5,
                        Order = 5,
                        Name = "career",
                        DisplayName = "Career",
                        TRT = TRTMappingPatient.Career,
                        AllowOther = true
                    },
                    new PatientHistoryItem
                    {
                        Id = -6,
                        Order = 6,
                        Name = "income",
                        DisplayName = "Income",
                        TRT = TRTMappingPatient.Income,
                        IsNumber = true,
                    },
                    new PatientHistoryItem
                    {
                        Id = -7,
                        Order = 7,
                        Name = "smoke",
                        DisplayName = "Smoke",
                        TRT = TRTMappingPatient.Smoke,
                        IsYesNo = true,
                    },
                    new PatientHistoryItem
                    {
                        Id = -8,
                        Order = 8,
                        Name = "drink",
                        DisplayName = "Drink",
                        TRT = TRTMappingPatient.Drink,
                        IsYesNo = true,
                    }
                );

                c.OwnsMany(x => x.Choices, b =>
                {
                    b.HasData(
                        new PatientChoice { Id = -99, PatientHistoryItemId = -3, Text = "Married" },
                        new PatientChoice { Id = -98, PatientHistoryItemId = -3, Text = "Divorced" },
                        new PatientChoice { Id = -97, PatientHistoryItemId = -3, Text = "Single" },

                        new PatientChoice { Id = -96, PatientHistoryItemId = -4, Text = "Primary School" },
                        new PatientChoice { Id = -95, PatientHistoryItemId = -4, Text = "Middle School" },
                        new PatientChoice { Id = -94, PatientHistoryItemId = -4, Text = "High School" },
                        new PatientChoice { Id = -93, PatientHistoryItemId = -4, Text = "Vocational Certificate" },
                        new PatientChoice { Id = -92, PatientHistoryItemId = -4, Text = "High Vocational Certificate" },
                        new PatientChoice { Id = -91, PatientHistoryItemId = -4, Text = "Diploma" },
                        new PatientChoice { Id = -90, PatientHistoryItemId = -4, Text = "Bachelor's Degree" },
                        new PatientChoice { Id = -89, PatientHistoryItemId = -4, Text = "Master's Degree" },
                        new PatientChoice { Id = -88, PatientHistoryItemId = -4, Text = "Doctor's Degree/ Ph.D." },

                        new PatientChoice { Id = -87, PatientHistoryItemId = -5, Text = "State Enterprise Employee" },
                        new PatientChoice { Id = -86, PatientHistoryItemId = -5, Text = "Business Owner" },
                        new PatientChoice { Id = -85, PatientHistoryItemId = -5, Text = "Goverment Officer" },
                        new PatientChoice { Id = -84, PatientHistoryItemId = -5, Text = "Employee" },
                        new PatientChoice { Id = -83, PatientHistoryItemId = -5, Text = "Freelance" }
                        );
                    b.WithOwner().HasForeignKey(x => x.PatientHistoryItemId);
                });
            });

            // Additional labs from csv
            builder.ApplyCsvSeed<LabExamItem>("labs");

            builder.ApplyCsvSeed<Underlying>("underlyings");

            builder.ApplyCsvSeed<Dialysate>("dialysates");
        }

        private static void InitAssessmentData(ModelBuilder builder)
        {
            builder.ApplyCsvSeed<Assessment, AssessmentMap>("assessments");
            builder.ApplyCsvSeed<AssessmentOption>("assessment-options");
        }

        // ------------- Util -----------------

        public class CsvBooleanConverter : BooleanConverter
        {
            public override object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
            {
                if (string.Equals(text, "t", StringComparison.OrdinalIgnoreCase))
                    return true;
                else if (string.Equals(text, "f", StringComparison.OrdinalIgnoreCase))
                    return false;

                return base.ConvertFromString(text, row, memberMapData);
            }
        }

        private static void ApplyCsvSeed<TEntity, TMapClass>(this ModelBuilder builder, string csvFileName) where TEntity : class where TMapClass : class
        {
            builder.ApplyCsvSeed<TEntity>(csvFileName, typeof(TMapClass));
        }
        private static void ApplyCsvSeed<TEntity>(this ModelBuilder builder, string csvFileName, Type registerClassMap = null) where TEntity : class
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            string resourceName = assembly.GetName().Name + $".{SeedFolder}.{csvFileName}.csv";
            using Stream stream = assembly.GetManifestResourceStream(resourceName);
            using StreamReader reader = new(stream, Encoding.UTF8);
            CsvReader csvReader = new(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HeaderValidated = null,
                MissingFieldFound = null
            });
            csvReader.Context.TypeConverterCache.AddConverter<bool>(new CsvBooleanConverter());
            if (registerClassMap != null)
            {
                csvReader.Context.RegisterClassMap(registerClassMap);
            }
            var seedData = csvReader.GetRecords<TEntity>().ToArray();
            builder.Entity<TEntity>()
                .HasData(seedData);
        }

        private sealed class AssessmentMap : ClassMap<Assessment>
        {
            public AssessmentMap()
            {
                var config = new CsvConfiguration(CultureInfo.CurrentCulture)
                {
                    IgnoreReferences = true
                };
                AutoMap(config);
            }
        }
    }
}
