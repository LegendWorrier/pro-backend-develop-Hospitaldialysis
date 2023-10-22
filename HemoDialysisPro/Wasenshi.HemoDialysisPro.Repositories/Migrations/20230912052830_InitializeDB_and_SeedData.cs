using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Wasenshi.HemoDialysisPro.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class InitializeDB_and_SeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Admissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    Updated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    AN = table.Column<string>(type: "text", nullable: false),
                    PatientId = table.Column<string>(type: "text", nullable: false),
                    Admit = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Discharged = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ChiefComplaint = table.Column<string>(type: "text", nullable: true),
                    Diagnosis = table.Column<string>(type: "text", nullable: true),
                    Room = table.Column<string>(type: "text", nullable: true),
                    TelNo = table.Column<string>(type: "text", nullable: true),
                    StatusDc = table.Column<string>(type: "text", nullable: true),
                    TransferTo = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Admissions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Anticoagulants",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    Updated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Anticoagulants", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AssessmentGroups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    Updated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true),
                    DisplayName = table.Column<string>(type: "text", nullable: true),
                    Order = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssessmentGroups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AvShuntIssueTreatments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    Updated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    PatientId = table.Column<string>(type: "text", nullable: false),
                    AbnormalDatetime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Complications = table.Column<string>(type: "text", nullable: false),
                    TreatmentMethod = table.Column<string>(type: "text", nullable: false),
                    Hospital = table.Column<string>(type: "text", nullable: true),
                    TreatmentResult = table.Column<string>(type: "text", nullable: false),
                    CathId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AvShuntIssueTreatments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AvShunts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    Updated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    PatientId = table.Column<string>(type: "text", nullable: false),
                    EstablishedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CatheterType = table.Column<int>(type: "integer", nullable: false),
                    Side = table.Column<int>(type: "integer", nullable: false),
                    ShuntSite = table.Column<string>(type: "text", nullable: false),
                    CatheterizationInstitution = table.Column<string>(type: "text", nullable: true),
                    Note = table.Column<string>(type: "text", nullable: true),
                    ReasonForDiscontinuation = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AvShunts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CauseOfDeath",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    Updated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CauseOfDeath", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Dialysates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    Updated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Ca = table.Column<float>(type: "real", nullable: false),
                    K = table.Column<float>(type: "real", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dialysates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DialysisPrescriptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    Updated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    PatientId = table.Column<string>(type: "text", nullable: false),
                    Temporary = table.Column<bool>(type: "boolean", nullable: false),
                    Mode = table.Column<int>(type: "integer", nullable: false),
                    HdfType = table.Column<int>(type: "integer", nullable: true),
                    SubstituteVolume = table.Column<float>(type: "real", nullable: true),
                    IvSupplementVolume = table.Column<float>(type: "real", nullable: true),
                    IvSupplementPosition = table.Column<string>(type: "text", nullable: true),
                    DryWeight = table.Column<float>(type: "real", nullable: true),
                    ExcessFluidRemovalAmount = table.Column<float>(type: "real", nullable: true),
                    BloodFlow = table.Column<float>(type: "real", nullable: true),
                    BloodTransfusion = table.Column<float>(type: "real", nullable: true),
                    ExtraFluid = table.Column<float>(type: "real", nullable: true),
                    Duration = table.Column<TimeSpan>(type: "interval", nullable: false),
                    Frequency = table.Column<short>(type: "smallint", nullable: true),
                    Anticoagulant = table.Column<string>(type: "text", nullable: true),
                    AcPerSession = table.Column<float>(type: "real", nullable: true),
                    InitialAmount = table.Column<float>(type: "real", nullable: true),
                    MaintainAmount = table.Column<float>(type: "real", nullable: true),
                    ReasonForRefraining = table.Column<string>(type: "text", nullable: true),
                    AcPerSessionMl = table.Column<float>(type: "real", nullable: true),
                    InitialAmountMl = table.Column<float>(type: "real", nullable: true),
                    MaintainAmountMl = table.Column<float>(type: "real", nullable: true),
                    DialysateK = table.Column<float>(type: "real", nullable: true),
                    DialysateCa = table.Column<float>(type: "real", nullable: true),
                    HCO3 = table.Column<float>(type: "real", nullable: true),
                    Na = table.Column<float>(type: "real", nullable: true),
                    DialysateTemperature = table.Column<float>(type: "real", nullable: true),
                    DialysateFlowRate = table.Column<float>(type: "real", nullable: true),
                    BloodAccessRoute = table.Column<string>(type: "text", nullable: true),
                    ANeedleCC = table.Column<float>(type: "real", nullable: true),
                    VNeedleCC = table.Column<float>(type: "real", nullable: true),
                    ArterialNeedle = table.Column<int>(type: "integer", nullable: true),
                    VenousNeedle = table.Column<int>(type: "integer", nullable: true),
                    Dialyzer = table.Column<string>(type: "text", nullable: true),
                    DialyzerSurfaceArea = table.Column<float>(type: "real", nullable: true),
                    AvgDialyzerReuse = table.Column<float>(type: "real", nullable: true),
                    DialysisNurse = table.Column<Guid>(type: "uuid", nullable: true),
                    Note = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DialysisPrescriptions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DialysisRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    Updated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    HemodialysisId = table.Column<Guid>(type: "uuid", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Remaining = table.Column<TimeSpan>(type: "interval", nullable: true),
                    Model = table.Column<string>(type: "text", nullable: true),
                    Number = table.Column<string>(type: "text", nullable: true),
                    BPS = table.Column<int>(type: "integer", nullable: true),
                    BPD = table.Column<int>(type: "integer", nullable: true),
                    HR = table.Column<int>(type: "integer", nullable: true),
                    RR = table.Column<int>(type: "integer", nullable: true),
                    Temp = table.Column<float>(type: "real", nullable: true),
                    BFR = table.Column<float>(type: "real", nullable: true),
                    VP = table.Column<int>(type: "integer", nullable: true),
                    AP = table.Column<int>(type: "integer", nullable: true),
                    DP = table.Column<int>(type: "integer", nullable: true),
                    TMP = table.Column<int>(type: "integer", nullable: true),
                    UFRate = table.Column<float>(type: "real", nullable: true),
                    UFTotal = table.Column<float>(type: "real", nullable: true),
                    AcLoading = table.Column<float>(type: "real", nullable: true),
                    AcMaintain = table.Column<float>(type: "real", nullable: true),
                    HAV = table.Column<float>(type: "real", nullable: true),
                    DFRTarget = table.Column<float>(type: "real", nullable: true),
                    DFR = table.Column<float>(type: "real", nullable: true),
                    Dialysate = table.Column<string>(type: "text", nullable: true),
                    DTTarget = table.Column<float>(type: "real", nullable: true),
                    DT = table.Column<float>(type: "real", nullable: true),
                    DCTarget = table.Column<float>(type: "real", nullable: true),
                    DC = table.Column<float>(type: "real", nullable: true),
                    BC = table.Column<float>(type: "real", nullable: true),
                    NSS = table.Column<float>(type: "real", nullable: true),
                    Glucose50 = table.Column<float>(type: "real", nullable: true),
                    HCO3 = table.Column<float>(type: "real", nullable: true),
                    NaTarget = table.Column<float>(type: "real", nullable: true),
                    NaProfile = table.Column<string>(type: "text", nullable: true),
                    UFProfile = table.Column<string>(type: "text", nullable: true),
                    Mode = table.Column<string>(type: "text", nullable: true),
                    BFAV = table.Column<float>(type: "real", nullable: true),
                    UFTarget = table.Column<float>(type: "real", nullable: true),
                    SRate = table.Column<float>(type: "real", nullable: true),
                    SAV = table.Column<float>(type: "real", nullable: true),
                    STarget = table.Column<float>(type: "real", nullable: true),
                    STemp = table.Column<float>(type: "real", nullable: true),
                    Ktv = table.Column<float>(type: "real", nullable: true),
                    PRR = table.Column<float>(type: "real", nullable: true),
                    URR = table.Column<float>(type: "real", nullable: true),
                    RecirculationRate = table.Column<int>(type: "integer", nullable: true),
                    DBV = table.Column<float>(type: "real", nullable: true),
                    Note = table.Column<string>(type: "text", nullable: true),
                    IsFromMachine = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DialysisRecords", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Dialyzers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    Updated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    BrandName = table.Column<string>(type: "text", nullable: true),
                    Flux = table.Column<int>(type: "integer", nullable: false),
                    Membrane = table.Column<int>(type: "integer", nullable: false),
                    SurfaceArea = table.Column<float>(type: "real", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Code = table.Column<string>(type: "text", nullable: true),
                    PieceUnit = table.Column<string>(type: "text", nullable: true),
                    Barcode = table.Column<string>(type: "text", nullable: true),
                    Note = table.Column<string>(type: "text", nullable: true),
                    Image = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dialyzers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DoctorRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    Updated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    HemodialysisId = table.Column<Guid>(type: "uuid", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DoctorRecords", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Equipments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    Updated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Code = table.Column<string>(type: "text", nullable: true),
                    PieceUnit = table.Column<string>(type: "text", nullable: true),
                    Barcode = table.Column<string>(type: "text", nullable: true),
                    Note = table.Column<string>(type: "text", nullable: true),
                    Image = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Equipments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Files",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    Updated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Uri = table.Column<string>(type: "text", nullable: true),
                    ContentType = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Files", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LabExamItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    Updated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Unit = table.Column<string>(type: "text", nullable: false),
                    IsYesNo = table.Column<bool>(type: "boolean", nullable: false),
                    Category = table.Column<int>(type: "integer", nullable: false),
                    UpperLimit = table.Column<float>(type: "real", nullable: true),
                    LowerLimit = table.Column<float>(type: "real", nullable: true),
                    UpperLimitM = table.Column<float>(type: "real", nullable: true),
                    LowerLimitM = table.Column<float>(type: "real", nullable: true),
                    UpperLimitF = table.Column<float>(type: "real", nullable: true),
                    LowerLimitF = table.Column<float>(type: "real", nullable: true),
                    TRT = table.Column<int>(type: "integer", nullable: false),
                    IsSystemBound = table.Column<bool>(type: "boolean", nullable: false),
                    Bound = table.Column<int>(type: "integer", nullable: true),
                    IsCalculated = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LabExamItems", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MedCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    Updated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MedicalSupplies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    Updated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Code = table.Column<string>(type: "text", nullable: true),
                    PieceUnit = table.Column<string>(type: "text", nullable: true),
                    Barcode = table.Column<string>(type: "text", nullable: true),
                    Note = table.Column<string>(type: "text", nullable: true),
                    Image = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedicalSupplies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Needles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    Updated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Number = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Needles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NurseRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    Updated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    HemodialysisId = table.Column<Guid>(type: "uuid", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NurseRecords", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PatientHistories",
                columns: table => new
                {
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    Updated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    PatientId = table.Column<string>(type: "text", nullable: false),
                    HistoryItemId = table.Column<int>(type: "integer", nullable: false),
                    NumberValue = table.Column<float>(type: "real", nullable: true),
                    Value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PatientHistories", x => new { x.PatientId, x.HistoryItemId });
                });

            migrationBuilder.CreateTable(
                name: "PatientHistoryItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    Updated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    DisplayName = table.Column<string>(type: "text", nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    IsYesNo = table.Column<bool>(type: "boolean", nullable: false),
                    IsNumber = table.Column<bool>(type: "boolean", nullable: false),
                    AllowOther = table.Column<bool>(type: "boolean", nullable: false),
                    TRT = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PatientHistoryItems", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Patients",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    Updated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    HospitalNumber = table.Column<string>(type: "text", nullable: false),
                    IdentityNo = table.Column<string>(type: "text", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Gender = table.Column<string>(type: "text", nullable: true),
                    BirthDate = table.Column<DateOnly>(type: "date", nullable: false),
                    BloodType = table.Column<string>(type: "text", nullable: true),
                    Telephone = table.Column<string>(type: "text", nullable: true),
                    Address = table.Column<string>(type: "text", nullable: true),
                    TransferFrom = table.Column<string>(type: "text", nullable: true),
                    Admission = table.Column<int>(type: "integer", nullable: true),
                    CoverageScheme = table.Column<int>(type: "integer", nullable: true),
                    DoctorId = table.Column<Guid>(type: "uuid", nullable: true),
                    UnitId = table.Column<int>(type: "integer", nullable: false),
                    Note = table.Column<string>(type: "text", nullable: true),
                    DialysisInfo_AccumulatedTreatmentTimes = table.Column<int>(type: "integer", nullable: true),
                    DialysisInfo_FirstTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DialysisInfo_FirstTimeAtHere = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DialysisInfo_EndDateAtHere = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DialysisInfo_KidneyTransplant = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DialysisInfo_KidneyState = table.Column<int>(type: "integer", nullable: true),
                    DialysisInfo_CauseOfKidneyDisease = table.Column<string>(type: "text", nullable: true),
                    DialysisInfo_Status = table.Column<string>(type: "text", nullable: true),
                    DialysisInfo_TimeOfDeath = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DialysisInfo_CauseOfDeath = table.Column<string>(type: "text", nullable: true),
                    DialysisInfo_TransferTo = table.Column<string>(type: "text", nullable: true),
                    EmergencyContact_Name = table.Column<string>(type: "text", nullable: true),
                    EmergencyContact_PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    EmergencyContact_Relationship = table.Column<string>(type: "text", nullable: true),
                    Barcode = table.Column<string>(type: "text", nullable: true),
                    RFID = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Patients", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ScheduleMeta",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    Updated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    UnitId = table.Column<int>(type: "integer", nullable: false),
                    UnitName = table.Column<string>(type: "text", nullable: true),
                    Section1 = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    Section2 = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    Section3 = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    Section4 = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    Section5 = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    Section6 = table.Column<TimeOnly>(type: "time without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduleMeta", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ShiftIncharges",
                columns: table => new
                {
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    Updated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    UnitId = table.Column<int>(type: "integer", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShiftIncharges", x => new { x.UnitId, x.Date });
                });

            migrationBuilder.CreateTable(
                name: "Status",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    Updated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Category = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Status", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StockItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    Updated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    UnitId = table.Column<int>(type: "integer", nullable: false),
                    EntryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    IsCredit = table.Column<bool>(type: "boolean", nullable: false),
                    PricePerPiece = table.Column<double>(type: "double precision", nullable: false),
                    StockType = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockItems", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TempSections",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    UnitId = table.Column<int>(type: "integer", nullable: false),
                    StartTime = table.Column<TimeSpan>(type: "time", nullable: true),
                    Delete = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TempSections", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Underlyings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    Updated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Underlyings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Units",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    Updated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Code = table.Column<string>(type: "text", nullable: true),
                    HeadNurse = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Units", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<string>(type: "text", nullable: true),
                    FirstName = table.Column<string>(type: "text", nullable: true),
                    LastName = table.Column<string>(type: "text", nullable: true),
                    Signature = table.Column<string>(type: "text", nullable: true),
                    IsPartTime = table.Column<bool>(type: "boolean", nullable: false),
                    UserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: true),
                    SecurityStamp = table.Column<string>(type: "text", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserShifts",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    Updated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Month = table.Column<DateOnly>(type: "date", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Suspended = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserShifts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Wards",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    Updated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    IsICU = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Wards", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Assessments",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    Updated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    GroupId = table.Column<int>(type: "integer", nullable: true),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true),
                    DisplayName = table.Column<string>(type: "text", nullable: true),
                    OptionType = table.Column<int>(type: "integer", nullable: false),
                    Multi = table.Column<bool>(type: "boolean", nullable: false),
                    HasOther = table.Column<bool>(type: "boolean", nullable: false),
                    HasText = table.Column<bool>(type: "boolean", nullable: false),
                    HasNumber = table.Column<bool>(type: "boolean", nullable: false),
                    Note = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Assessments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Assessments_AssessmentGroups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "AssessmentGroups",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "HemodialysisRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    Updated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    PatientId = table.Column<string>(type: "text", nullable: false),
                    CompletedTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Admission = table.Column<int>(type: "integer", nullable: false),
                    OutsideUnit = table.Column<bool>(type: "boolean", nullable: false),
                    Ward = table.Column<string>(type: "text", nullable: true),
                    Bed = table.Column<string>(type: "text", nullable: true),
                    CycleStartTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CycleEndTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsICU = table.Column<bool>(type: "boolean", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    AcNotUsed = table.Column<bool>(type: "boolean", nullable: false),
                    ReasonForRefraining = table.Column<string>(type: "text", nullable: true),
                    FlushNSS = table.Column<float>(type: "real", nullable: true),
                    FlushNSSInterval = table.Column<int>(type: "integer", nullable: true),
                    FlushTimes = table.Column<int>(type: "integer", nullable: true),
                    Dehydration_LastPostWeight = table.Column<float>(type: "real", nullable: true),
                    Dehydration_CheckInTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Dehydration_PreTotalWeight = table.Column<float>(type: "real", nullable: true),
                    Dehydration_WheelchairWeight = table.Column<float>(type: "real", nullable: true),
                    Dehydration_ClothWeight = table.Column<float>(type: "real", nullable: true),
                    Dehydration_FoodDrinkWeight = table.Column<float>(type: "real", nullable: true),
                    Dehydration_BloodTransfusion = table.Column<float>(type: "real", nullable: true),
                    Dehydration_ExtraFluid = table.Column<float>(type: "real", nullable: true),
                    Dehydration_UFGoal = table.Column<float>(type: "real", nullable: true),
                    Dehydration_PostTotalWeight = table.Column<float>(type: "real", nullable: true),
                    Dehydration_PostWheelchairWeight = table.Column<float>(type: "real", nullable: true),
                    Dehydration_Abnormal = table.Column<bool>(type: "boolean", nullable: true),
                    Dehydration_Reason = table.Column<string>(type: "text", nullable: true),
                    DialysisPrescriptionId = table.Column<Guid>(type: "uuid", nullable: true),
                    Dialyzer_UseNo = table.Column<int>(type: "integer", nullable: true),
                    Dialyzer_TCV = table.Column<float>(type: "real", nullable: true),
                    BloodCollection_Pre = table.Column<string>(type: "text", nullable: true),
                    BloodCollection_Post = table.Column<string>(type: "text", nullable: true),
                    AvShunt_AVShuntId = table.Column<Guid>(type: "uuid", nullable: true),
                    AvShunt_ShuntSite = table.Column<string>(type: "text", nullable: true),
                    AvShunt_ALength = table.Column<float>(type: "real", nullable: true),
                    AvShunt_VLength = table.Column<float>(type: "real", nullable: true),
                    AvShunt_ANeedleCC = table.Column<float>(type: "real", nullable: true),
                    AvShunt_VNeedleCC = table.Column<float>(type: "real", nullable: true),
                    AvShunt_ASize = table.Column<int>(type: "integer", nullable: true),
                    AvShunt_VSize = table.Column<int>(type: "integer", nullable: true),
                    AvShunt_ANeedleTimes = table.Column<short>(type: "smallint", nullable: true),
                    AvShunt_VNeedleTimes = table.Column<short>(type: "smallint", nullable: true),
                    ProofReader = table.Column<Guid>(type: "uuid", nullable: true),
                    DoctorConsent = table.Column<bool>(type: "boolean", nullable: false),
                    ShiftSectionId = table.Column<int>(type: "integer", nullable: false),
                    NursesInShift = table.Column<Guid[]>(type: "uuid[]", nullable: true),
                    TreatmentNo = table.Column<int>(type: "integer", nullable: true),
                    DoctorId = table.Column<Guid>(type: "uuid", nullable: true),
                    SentPDF = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HemodialysisRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HemodialysisRecords_DialysisPrescriptions_DialysisPrescript~",
                        column: x => x.DialysisPrescriptionId,
                        principalTable: "DialysisPrescriptions",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "LabHemosheets",
                columns: table => new
                {
                    LabItemId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    OnlyOnDate = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LabHemosheets", x => x.LabItemId);
                    table.ForeignKey(
                        name: "FK_LabHemosheets_LabExamItems_LabItemId",
                        column: x => x.LabItemId,
                        principalTable: "LabExamItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Medicines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    Updated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CategoryId = table.Column<int>(type: "integer", nullable: true),
                    UsageWays = table.Column<int>(type: "integer", nullable: false),
                    Dose = table.Column<float>(type: "real", nullable: true),
                    MedType = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Code = table.Column<string>(type: "text", nullable: true),
                    PieceUnit = table.Column<string>(type: "text", nullable: true),
                    Barcode = table.Column<string>(type: "text", nullable: true),
                    Note = table.Column<string>(type: "text", nullable: true),
                    Image = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Medicines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Medicines_MedCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "MedCategories",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PatientChoice",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PatientHistoryItemId = table.Column<int>(type: "integer", nullable: false),
                    Text = table.Column<string>(type: "text", nullable: true),
                    NumberValue = table.Column<float>(type: "real", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PatientChoice", x => new { x.PatientHistoryItemId, x.Id });
                    table.ForeignKey(
                        name: "FK_PatientChoice_PatientHistoryItems_PatientHistoryItemId",
                        column: x => x.PatientHistoryItemId,
                        principalTable: "PatientHistoryItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LabExams",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    Updated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    EntryTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PatientId = table.Column<string>(type: "text", nullable: false),
                    LabItemId = table.Column<int>(type: "integer", nullable: false),
                    LabValue = table.Column<float>(type: "real", nullable: false),
                    Note = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LabExams", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LabExams_LabExamItems_LabItemId",
                        column: x => x.LabItemId,
                        principalTable: "LabExamItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LabExams_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tags",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    Updated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    PatientId = table.Column<string>(type: "text", nullable: false),
                    Text = table.Column<string>(type: "text", nullable: true),
                    Color = table.Column<string>(type: "text", nullable: true),
                    Italic = table.Column<bool>(type: "boolean", nullable: false),
                    Bold = table.Column<bool>(type: "boolean", nullable: false),
                    StrikeThroughStyle = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tags", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tags_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ShiftMeta",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    Updated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Month = table.Column<DateOnly>(type: "date", nullable: false),
                    ScheduleMetaId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShiftMeta", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShiftMeta_ScheduleMeta_ScheduleMetaId",
                        column: x => x.ScheduleMetaId,
                        principalTable: "ScheduleMeta",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DialyzerStock",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ItemId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DialyzerStock", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DialyzerStock_Dialyzers_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Dialyzers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DialyzerStock_StockItems_Id",
                        column: x => x.Id,
                        principalTable: "StockItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EquipmentStock",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ItemId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EquipmentStock", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EquipmentStock_Equipments_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Equipments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EquipmentStock_StockItems_Id",
                        column: x => x.Id,
                        principalTable: "StockItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MedicalSupplyStock",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ItemId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedicalSupplyStock", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MedicalSupplyStock_MedicalSupplies_ItemId",
                        column: x => x.ItemId,
                        principalTable: "MedicalSupplies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MedicalSupplyStock_StockItems_Id",
                        column: x => x.Id,
                        principalTable: "StockItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AdmissionUnderlyings",
                columns: table => new
                {
                    AdmissionId = table.Column<Guid>(type: "uuid", nullable: false),
                    UnderlyingId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdmissionUnderlyings", x => new { x.AdmissionId, x.UnderlyingId });
                    table.ForeignKey(
                        name: "Admission_Underlying",
                        column: x => x.AdmissionId,
                        principalTable: "Admissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "Underlying_Underlying",
                        column: x => x.UnderlyingId,
                        principalTable: "Underlyings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Sections",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    Updated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    UnitId = table.Column<int>(type: "integer", nullable: false),
                    StartTime = table.Column<TimeOnly>(type: "time without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Sections_Units_UnitId",
                        column: x => x.UnitId,
                        principalTable: "Units",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    ProviderKey = table.Column<string>(type: "text", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserUnits",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    UnitId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserUnits", x => new { x.UnitId, x.UserId });
                    table.ForeignKey(
                        name: "FK_UserUnits_Units_UnitId",
                        column: x => x.UnitId,
                        principalTable: "Units",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserUnits_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AssessmentOptions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    Updated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    AssessmentId = table.Column<long>(type: "bigint", nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true),
                    DisplayName = table.Column<string>(type: "text", nullable: true),
                    Note = table.Column<string>(type: "text", nullable: true),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false),
                    TextValue = table.Column<string>(type: "text", nullable: true),
                    Value = table.Column<float>(type: "real", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssessmentOptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssessmentOptions_Assessments_AssessmentId",
                        column: x => x.AssessmentId,
                        principalTable: "Assessments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DialysisRecordAssessmentItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    Updated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    DialysisRecordId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssessmentId = table.Column<long>(type: "bigint", nullable: false),
                    Selected = table.Column<long[]>(type: "bigint[]", nullable: true),
                    Checked = table.Column<bool>(type: "boolean", nullable: false),
                    Text = table.Column<string>(type: "text", nullable: true),
                    Value = table.Column<float>(type: "real", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DialysisRecordAssessmentItems", x => x.Id);
                    table.ForeignKey(
                        name: "DialysisRecord",
                        column: x => x.DialysisRecordId,
                        principalTable: "DialysisRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DialysisRecordAssessmentItems_Assessments_AssessmentId",
                        column: x => x.AssessmentId,
                        principalTable: "Assessments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AssessmentItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    Updated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    HemosheetId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsReassessment = table.Column<bool>(type: "boolean", nullable: false),
                    AssessmentId = table.Column<long>(type: "bigint", nullable: false),
                    Selected = table.Column<long[]>(type: "bigint[]", nullable: true),
                    Checked = table.Column<bool>(type: "boolean", nullable: false),
                    Text = table.Column<string>(type: "text", nullable: true),
                    Value = table.Column<float>(type: "real", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssessmentItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssessmentItems_Assessments_AssessmentId",
                        column: x => x.AssessmentId,
                        principalTable: "Assessments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "Hemosheet",
                        column: x => x.HemosheetId,
                        principalTable: "HemodialysisRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HemodialysisRecords_PostVitalsign",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    BPS = table.Column<int>(type: "integer", nullable: false),
                    BPD = table.Column<int>(type: "integer", nullable: false),
                    HR = table.Column<int>(type: "integer", nullable: false),
                    RR = table.Column<int>(type: "integer", nullable: false),
                    Temp = table.Column<float>(type: "real", nullable: false),
                    SpO2 = table.Column<float>(type: "real", nullable: false),
                    Posture = table.Column<int>(type: "integer", nullable: true),
                    HemodialysisRecordId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HemodialysisRecords_PostVitalsign", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HemodialysisRecords_PostVitalsign_HemodialysisRecords_Hemod~",
                        column: x => x.HemodialysisRecordId,
                        principalTable: "HemodialysisRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HemodialysisRecords_PreVitalsign",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    BPS = table.Column<int>(type: "integer", nullable: false),
                    BPD = table.Column<int>(type: "integer", nullable: false),
                    HR = table.Column<int>(type: "integer", nullable: false),
                    RR = table.Column<int>(type: "integer", nullable: false),
                    Temp = table.Column<float>(type: "real", nullable: false),
                    SpO2 = table.Column<float>(type: "real", nullable: false),
                    Posture = table.Column<int>(type: "integer", nullable: true),
                    HemodialysisRecordId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HemodialysisRecords_PreVitalsign", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HemodialysisRecords_PreVitalsign_HemodialysisRecords_Hemodi~",
                        column: x => x.HemodialysisRecordId,
                        principalTable: "HemodialysisRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HemoNotes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    Updated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    HemoId = table.Column<Guid>(type: "uuid", nullable: false),
                    Complication = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HemoNotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HemoNotes_HemodialysisRecords_HemoId",
                        column: x => x.HemoId,
                        principalTable: "HemodialysisRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProgressNotes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    Updated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    HemodialysisId = table.Column<Guid>(type: "uuid", nullable: false),
                    Order = table.Column<short>(type: "smallint", nullable: false),
                    Focus = table.Column<string>(type: "text", nullable: false),
                    A = table.Column<string>(type: "text", nullable: true),
                    I = table.Column<string>(type: "text", nullable: true),
                    E = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProgressNotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProgressNotes_HemodialysisRecords_HemodialysisId",
                        column: x => x.HemodialysisId,
                        principalTable: "HemodialysisRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MedicineHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    Updated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    EntryTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PatientId = table.Column<string>(type: "text", nullable: false),
                    MedicineId = table.Column<int>(type: "integer", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    OverrideDose = table.Column<float>(type: "real", nullable: true),
                    OverrideUnit = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedicineHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MedicineHistories_Medicines_MedicineId",
                        column: x => x.MedicineId,
                        principalTable: "Medicines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MedicinePrescriptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    Updated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    PatientId = table.Column<string>(type: "text", nullable: false),
                    MedicineId = table.Column<int>(type: "integer", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    Route = table.Column<int>(type: "integer", nullable: false),
                    Frequency = table.Column<int>(type: "integer", nullable: false),
                    AdministerDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Duration = table.Column<int>(type: "integer", nullable: false),
                    HospitalName = table.Column<string>(type: "text", nullable: true),
                    OverrideDose = table.Column<float>(type: "real", nullable: true),
                    OverrideUnit = table.Column<string>(type: "text", nullable: true),
                    Note = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedicinePrescriptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MedicinePrescriptions_Medicines_MedicineId",
                        column: x => x.MedicineId,
                        principalTable: "Medicines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MedicineStock",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ItemId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedicineStock", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MedicineStock_Medicines_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Medicines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MedicineStock_StockItems_Id",
                        column: x => x.Id,
                        principalTable: "StockItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PatientMedicine",
                columns: table => new
                {
                    PatientId = table.Column<string>(type: "text", nullable: false),
                    MedicineId = table.Column<int>(type: "integer", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PatientMedicine", x => new { x.PatientId, x.MedicineId });
                    table.ForeignKey(
                        name: "Medicine_Allergy",
                        column: x => x.MedicineId,
                        principalTable: "Medicines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "Patient_Allergy",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ShiftSlots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    Updated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ShiftMetaId = table.Column<long>(type: "bigint", nullable: true),
                    Data = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShiftSlots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShiftSlots_ShiftMeta_ShiftMetaId",
                        column: x => x.ShiftMetaId,
                        principalTable: "ShiftMeta",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Schedules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    Updated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    PatientId = table.Column<string>(type: "text", nullable: false),
                    SectionId = table.Column<int>(type: "integer", nullable: false),
                    Slot = table.Column<int>(type: "integer", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    OverrideUnitId = table.Column<int>(type: "integer", nullable: true),
                    OriginalDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Schedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Schedules_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Schedules_Sections_SectionId",
                        column: x => x.SectionId,
                        principalTable: "Sections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SectionSlotPatients",
                columns: table => new
                {
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    Updated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    SectionId = table.Column<int>(type: "integer", nullable: false),
                    PatientId = table.Column<string>(type: "text", nullable: false),
                    Slot = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SectionSlotPatients", x => new { x.PatientId, x.SectionId, x.Slot });
                    table.ForeignKey(
                        name: "FK_SectionSlotPatients_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SectionSlotPatients_Sections_SectionId",
                        column: x => x.SectionId,
                        principalTable: "Sections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ShiftInchargeSection",
                columns: table => new
                {
                    ShiftInchargeUnitId = table.Column<int>(type: "integer", nullable: false),
                    ShiftInchargeDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SectionId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShiftInchargeSection", x => new { x.ShiftInchargeUnitId, x.ShiftInchargeDate, x.Id });
                    table.ForeignKey(
                        name: "FK_ShiftInchargeSection_Sections_SectionId",
                        column: x => x.SectionId,
                        principalTable: "Sections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ShiftInchargeSection_ShiftIncharges_ShiftInchargeUnitId_Shi~",
                        columns: x => new { x.ShiftInchargeUnitId, x.ShiftInchargeDate },
                        principalTable: "ShiftIncharges",
                        principalColumns: new[] { "UnitId", "Date" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExecutionRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    Updated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    HemodialysisId = table.Column<Guid>(type: "uuid", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    IsExecuted = table.Column<bool>(type: "boolean", nullable: false),
                    CoSign = table.Column<Guid>(type: "uuid", nullable: true),
                    RecordId = table.Column<Guid>(type: "uuid", nullable: true),
                    PrescriptionId = table.Column<Guid>(type: "uuid", nullable: true),
                    OverrideRoute = table.Column<int>(type: "integer", nullable: true),
                    OverrideDose = table.Column<float>(type: "real", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExecutionRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExecutionRecords_HemodialysisRecords_HemodialysisId",
                        column: x => x.HemodialysisId,
                        principalTable: "HemodialysisRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExecutionRecords_MedicinePrescriptions_PrescriptionId",
                        column: x => x.PrescriptionId,
                        principalTable: "MedicinePrescriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Anticoagulants",
                columns: new[] { "Id", "Created", "CreatedBy", "IsActive", "Name", "Updated", "UpdatedBy" },
                values: new object[,]
                {
                    { -2, null, new Guid("00000000-0000-0000-0000-000000000000"), true, "Clexane", null, null },
                    { -1, null, new Guid("00000000-0000-0000-0000-000000000000"), true, "Heparin", null, null }
                });

            migrationBuilder.InsertData(
                table: "Assessments",
                columns: new[] { "Id", "Created", "CreatedBy", "DisplayName", "GroupId", "HasNumber", "HasOther", "HasText", "IsActive", "Multi", "Name", "Note", "OptionType", "Order", "Type", "Updated", "UpdatedBy" },
                values: new object[,]
                {
                    { -24L, null, new Guid("00000000-0000-0000-0000-000000000000"), "Health Education", null, false, false, false, true, true, "health", null, 1, 3, 1, null, null },
                    { -23L, null, new Guid("00000000-0000-0000-0000-000000000000"), "Nursing Intervention", null, false, false, false, true, true, "nursing", null, 1, 2, 1, null, null },
                    { -22L, null, new Guid("00000000-0000-0000-0000-000000000000"), "Technical Complication", null, false, false, false, true, true, "technical", null, 1, 1, 1, null, null },
                    { -21L, null, new Guid("00000000-0000-0000-0000-000000000000"), "Complication", null, false, false, false, true, true, "complication", null, 1, 0, 1, null, null },
                    { -20L, null, new Guid("00000000-0000-0000-0000-000000000000"), "High risk of fall due location", null, false, false, false, true, false, "fall", null, 1, 19, 0, null, null },
                    { -19L, null, new Guid("00000000-0000-0000-0000-000000000000"), "Machine Test", null, false, false, false, true, false, "machine", null, 1, 18, 0, null, null },
                    { -18L, null, new Guid("00000000-0000-0000-0000-000000000000"), "Bruit", null, false, false, false, true, true, "bruit", null, 1, 17, 0, null, null },
                    { -17L, null, new Guid("00000000-0000-0000-0000-000000000000"), "Thrill", null, false, false, false, true, false, "thrill", null, 1, 16, 0, null, null },
                    { -16L, null, new Guid("00000000-0000-0000-0000-000000000000"), "Inflammation", null, false, false, false, true, false, "inflame", null, 1, 15, 0, null, null },
                    { -15L, null, new Guid("00000000-0000-0000-0000-000000000000"), "Other", null, false, false, false, true, false, "other", null, 1, 14, 0, null, null },
                    { -14L, null, new Guid("00000000-0000-0000-0000-000000000000"), "Psychosocial problem", null, false, false, false, true, false, "psycho", null, 1, 13, 0, null, null },
                    { -13L, null, new Guid("00000000-0000-0000-0000-000000000000"), "Poor oral intake", null, false, false, false, true, false, "oral", null, 1, 12, 0, null, null },
                    { -12L, null, new Guid("00000000-0000-0000-0000-000000000000"), "Edema", null, false, false, false, true, false, "edema", null, 1, 11, 0, null, null },
                    { -11L, null, new Guid("00000000-0000-0000-0000-000000000000"), "Pale", null, false, false, false, true, false, "pale", null, 1, 10, 0, null, null },
                    { -10L, null, new Guid("00000000-0000-0000-0000-000000000000"), "Engorged neck vein", null, false, false, false, true, false, "neck", null, 1, 9, 0, null, null },
                    { -9L, null, new Guid("00000000-0000-0000-0000-000000000000"), "Itching", null, false, false, false, true, false, "itching", null, 1, 8, 0, null, null },
                    { -8L, null, new Guid("00000000-0000-0000-0000-000000000000"), "Bleeding", null, false, false, false, true, false, "bleeding", null, 1, 7, 0, null, null },
                    { -7L, null, new Guid("00000000-0000-0000-0000-000000000000"), "Sleep disturbance", null, false, false, false, true, false, "sleep", null, 1, 6, 0, null, null },
                    { -6L, null, new Guid("00000000-0000-0000-0000-000000000000"), "Nausea/Vomit", null, false, false, false, true, false, "vomit", null, 1, 5, 0, null, null },
                    { -5L, null, new Guid("00000000-0000-0000-0000-000000000000"), "Headache", null, false, false, false, true, false, "head", null, 1, 4, 0, null, null },
                    { -4L, null, new Guid("00000000-0000-0000-0000-000000000000"), "Fever", null, false, false, false, true, false, "fever", null, 1, 3, 0, null, null },
                    { -3L, null, new Guid("00000000-0000-0000-0000-000000000000"), "Dyspnea", null, false, false, false, true, false, "dyspnea", null, 1, 2, 0, null, null },
                    { -2L, null, new Guid("00000000-0000-0000-0000-000000000000"), "Chest discomfort", null, false, false, false, true, false, "chest", null, 1, 1, 0, null, null },
                    { -1L, null, new Guid("00000000-0000-0000-0000-000000000000"), "Pain", null, false, false, false, true, false, "pain", null, 1, 0, 0, null, null }
                });

            migrationBuilder.InsertData(
                table: "CauseOfDeath",
                columns: new[] { "Id", "Created", "CreatedBy", "IsActive", "Name", "Updated", "UpdatedBy" },
                values: new object[,]
                {
                    { -2, null, new Guid("00000000-0000-0000-0000-000000000000"), true, "Hypertension", null, null },
                    { -1, null, new Guid("00000000-0000-0000-0000-000000000000"), true, "Heart Failure", null, null }
                });

            migrationBuilder.InsertData(
                table: "Dialysates",
                columns: new[] { "Id", "Ca", "Created", "CreatedBy", "IsActive", "K", "Updated", "UpdatedBy" },
                values: new object[,]
                {
                    { -14, 2.5f, null, new Guid("00000000-0000-0000-0000-000000000000"), true, 0f, null, null },
                    { -13, 3.5f, null, new Guid("00000000-0000-0000-0000-000000000000"), true, 0f, null, null },
                    { -12, 2.5f, null, new Guid("00000000-0000-0000-0000-000000000000"), true, 1f, null, null },
                    { -11, 3.5f, null, new Guid("00000000-0000-0000-0000-000000000000"), true, 1f, null, null },
                    { -10, 2f, null, new Guid("00000000-0000-0000-0000-000000000000"), true, 2f, null, null },
                    { -9, 2.5f, null, new Guid("00000000-0000-0000-0000-000000000000"), true, 2f, null, null },
                    { -8, 3f, null, new Guid("00000000-0000-0000-0000-000000000000"), true, 2f, null, null },
                    { -7, 3.5f, null, new Guid("00000000-0000-0000-0000-000000000000"), true, 2f, null, null },
                    { -6, 2.5f, null, new Guid("00000000-0000-0000-0000-000000000000"), true, 2.5f, null, null },
                    { -5, 3.5f, null, new Guid("00000000-0000-0000-0000-000000000000"), true, 2.5f, null, null },
                    { -4, 2f, null, new Guid("00000000-0000-0000-0000-000000000000"), true, 3f, null, null },
                    { -3, 2.5f, null, new Guid("00000000-0000-0000-0000-000000000000"), true, 3f, null, null },
                    { -2, 3f, null, new Guid("00000000-0000-0000-0000-000000000000"), true, 3f, null, null },
                    { -1, 3.5f, null, new Guid("00000000-0000-0000-0000-000000000000"), true, 3f, null, null }
                });

            migrationBuilder.InsertData(
                table: "Dialyzers",
                columns: new[] { "Id", "Barcode", "BrandName", "Code", "Created", "CreatedBy", "Flux", "Image", "IsActive", "Membrane", "Name", "Note", "PieceUnit", "SurfaceArea", "Updated", "UpdatedBy" },
                values: new object[,]
                {
                    { -2, null, "Nikkiso", null, null, new Guid("00000000-0000-0000-0000-000000000000"), 1, null, true, 2, "FDY-21", null, null, 2.1f, null, null },
                    { -1, null, "Nikkiso", null, null, new Guid("00000000-0000-0000-0000-000000000000"), 1, null, true, 2, "FDX-21", null, null, 2.1f, null, null }
                });

            migrationBuilder.InsertData(
                table: "LabExamItems",
                columns: new[] { "Id", "Bound", "Category", "Created", "CreatedBy", "IsActive", "IsCalculated", "IsSystemBound", "IsYesNo", "LowerLimit", "LowerLimitF", "LowerLimitM", "Name", "TRT", "Unit", "Updated", "UpdatedBy", "UpperLimit", "UpperLimitF", "UpperLimitM" },
                values: new object[,]
                {
                    { -3, 2, 0, null, new Guid("00000000-0000-0000-0000-000000000000"), true, true, true, false, null, null, null, "URR", 0, "%", null, null, null, null, null },
                    { -2, 1, 0, null, new Guid("00000000-0000-0000-0000-000000000000"), true, true, true, false, null, null, null, "Kt/V", 0, "", null, null, null, null, null },
                    { -1, 0, 0, null, new Guid("00000000-0000-0000-0000-000000000000"), true, false, true, false, 6f, null, null, "BUN", 0, "mg/dL", null, null, 25f, null, null },
                    { 1, null, 3, null, new Guid("00000000-0000-0000-0000-000000000000"), true, false, false, false, null, null, null, "Albumin", 13, "g/dL", null, null, null, null, null },
                    { 2, null, 6, null, new Guid("00000000-0000-0000-0000-000000000000"), true, false, false, true, 0f, null, null, "Anti HBc", 27, "-", null, null, 1f, null, null },
                    { 3, null, 6, null, new Guid("00000000-0000-0000-0000-000000000000"), true, false, false, true, 0f, null, null, "Anti HBs", 26, "-", null, null, 1f, null, null },
                    { 4, null, 6, null, new Guid("00000000-0000-0000-0000-000000000000"), true, false, false, true, 0f, null, null, "Anti HCV", 28, "-", null, null, 1f, null, null },
                    { 5, null, 6, null, new Guid("00000000-0000-0000-0000-000000000000"), true, false, false, true, 0f, null, null, "Anti HIV", 29, "-", null, null, 1f, null, null },
                    { 6, null, 1, null, new Guid("00000000-0000-0000-0000-000000000000"), true, false, false, false, null, null, null, "AP", 12, "U/L", null, null, null, null, null },
                    { 7, null, 3, null, new Guid("00000000-0000-0000-0000-000000000000"), true, false, false, false, null, null, null, "Ca", 9, "mg/dL", null, null, null, null, null },
                    { 8, null, 6, null, new Guid("00000000-0000-0000-0000-000000000000"), true, false, false, false, null, null, null, "Cholesterol", 15, "mg/dL", null, null, null, null, null },
                    { 9, null, 3, null, new Guid("00000000-0000-0000-0000-000000000000"), true, false, false, false, null, null, null, "Cl", 7, "mmol/L", null, null, null, null, null },
                    { 10, null, 3, null, new Guid("00000000-0000-0000-0000-000000000000"), true, false, false, false, null, null, null, "CO2", 0, "mmol/L", null, null, null, null, null },
                    { 11, null, 3, null, new Guid("00000000-0000-0000-0000-000000000000"), true, false, false, false, null, 0.5f, 0.6f, "Cr", 3, "mg/dL", null, null, null, 1.1f, 1.2f },
                    { 12, null, 1, null, new Guid("00000000-0000-0000-0000-000000000000"), true, false, false, false, null, null, null, "DB", 0, "mg/dL", null, null, null, null, null },
                    { 13, null, 1, null, new Guid("00000000-0000-0000-0000-000000000000"), true, false, false, false, 90f, null, null, "eGFR", 0, "ml/min/1.73m²", null, null, null, null, null },
                    { 14, null, 6, null, new Guid("00000000-0000-0000-0000-000000000000"), true, false, false, false, null, null, null, "Ferritin", 23, "ng/ml", null, null, null, null, null },
                    { 15, null, 1, null, new Guid("00000000-0000-0000-0000-000000000000"), true, false, false, false, null, null, null, "FPG", 1, "mg/dL", null, null, null, null, null },
                    { 16, null, 1, null, new Guid("00000000-0000-0000-0000-000000000000"), true, false, false, false, null, null, null, "Globulin", 0, "g/dL", null, null, null, null, null },
                    { 17, null, 1, null, new Guid("00000000-0000-0000-0000-000000000000"), true, false, false, false, null, 12f, 13f, "Hb", 20, "g/dL", null, null, null, 16f, 17.4f },
                    { 18, null, 6, null, new Guid("00000000-0000-0000-0000-000000000000"), true, false, false, false, null, null, null, "HbA1C", 2, "%", null, null, null, null, null },
                    { 19, null, 6, null, new Guid("00000000-0000-0000-0000-000000000000"), true, false, false, true, 0f, null, null, "HBsAg", 25, "-", null, null, 1f, null, null },
                    { 20, null, 1, null, new Guid("00000000-0000-0000-0000-000000000000"), true, false, false, false, null, 35f, 40f, "Hct", 19, "%", null, null, null, 47f, 50f },
                    { 21, null, 6, null, new Guid("00000000-0000-0000-0000-000000000000"), true, false, false, false, null, null, null, "HDL", 17, "mg/dL", null, null, null, null, null },
                    { 22, null, 6, null, new Guid("00000000-0000-0000-0000-000000000000"), true, false, false, false, null, null, null, "IPTH", 11, "pg/ml", null, null, null, null, null },
                    { 23, null, 3, null, new Guid("00000000-0000-0000-0000-000000000000"), true, false, false, false, null, null, null, "K", 6, "mmol/L", null, null, null, null, null },
                    { 24, null, 6, null, new Guid("00000000-0000-0000-0000-000000000000"), true, false, false, false, null, null, null, "LDL", 18, "mg/dL", null, null, null, null, null },
                    { 25, null, 3, null, new Guid("00000000-0000-0000-0000-000000000000"), true, false, false, false, null, null, null, "Na", 5, "mmol/L", null, null, null, null, null },
                    { 26, null, 3, null, new Guid("00000000-0000-0000-0000-000000000000"), true, false, false, false, 1f, null, null, "nPCR", 0, "g/kg/d", null, null, 1.2f, null, null },
                    { 27, null, 1, null, new Guid("00000000-0000-0000-0000-000000000000"), true, false, false, false, null, null, null, "Platelet count", 0, "10³/μL", null, null, null, null, null },
                    { 28, null, 3, null, new Guid("00000000-0000-0000-0000-000000000000"), true, false, false, false, null, null, null, "PO4", 10, "mg/dL", null, null, null, null, null },
                    { 29, null, 6, null, new Guid("00000000-0000-0000-0000-000000000000"), true, false, false, false, null, null, null, "Serum iron", 21, "μg/dl", null, null, null, null, null },
                    { 30, null, 1, null, new Guid("00000000-0000-0000-0000-000000000000"), true, false, false, false, null, null, null, "SGOT", 0, "U/L", null, null, null, null, null },
                    { 31, null, 1, null, new Guid("00000000-0000-0000-0000-000000000000"), true, false, false, false, null, null, null, "SGPT", 0, "U/L", null, null, null, null, null },
                    { 32, null, 1, null, new Guid("00000000-0000-0000-0000-000000000000"), true, false, false, false, null, null, null, "spKT/V", 0, "-", null, null, null, null, null },
                    { 33, null, 1, null, new Guid("00000000-0000-0000-0000-000000000000"), true, false, false, false, null, null, null, "Stool occult blood", 0, "-", null, null, null, null, null },
                    { 34, null, 1, null, new Guid("00000000-0000-0000-0000-000000000000"), true, false, false, false, null, null, null, "TB", 0, "mg/dL", null, null, null, null, null },
                    { 35, null, 6, null, new Guid("00000000-0000-0000-0000-000000000000"), true, false, false, false, null, null, null, "TIBC", 22, "μg/dl", null, null, null, null, null },
                    { 36, null, 1, null, new Guid("00000000-0000-0000-0000-000000000000"), true, false, false, false, null, null, null, "Total protein", 14, "g/dL", null, null, null, null, null },
                    { 37, null, 6, null, new Guid("00000000-0000-0000-0000-000000000000"), true, false, false, false, null, null, null, "Triglyceride", 16, "mg/dL", null, null, null, null, null },
                    { 38, null, 6, null, new Guid("00000000-0000-0000-0000-000000000000"), true, false, false, false, null, null, null, "TSAT", 24, "%", null, null, null, null, null },
                    { 39, null, 3, null, new Guid("00000000-0000-0000-0000-000000000000"), true, false, false, false, null, null, null, "Uric acid", 4, "mg/dL", null, null, null, null, null },
                    { 40, null, 6, null, new Guid("00000000-0000-0000-0000-000000000000"), true, false, false, false, null, null, null, "VLDL", 0, "mg/dL", null, null, null, null, null },
                    { 41, null, 1, null, new Guid("00000000-0000-0000-0000-000000000000"), true, false, false, false, null, null, null, "WBC", 0, "10³/μL", null, null, null, null, null }
                });

            migrationBuilder.InsertData(
                table: "MedCategories",
                columns: new[] { "Id", "Created", "CreatedBy", "IsActive", "Name", "Updated", "UpdatedBy" },
                values: new object[,]
                {
                    { -4, null, new Guid("00000000-0000-0000-0000-000000000000"), true, "ST and Injection", null, null },
                    { -3, null, new Guid("00000000-0000-0000-0000-000000000000"), true, "Insulin", null, null },
                    { -2, null, new Guid("00000000-0000-0000-0000-000000000000"), true, "Injection", null, null },
                    { -1, null, new Guid("00000000-0000-0000-0000-000000000000"), true, "Oral", null, null }
                });

            migrationBuilder.InsertData(
                table: "Medicines",
                columns: new[] { "Id", "Barcode", "CategoryId", "Code", "Created", "CreatedBy", "Dose", "Image", "IsActive", "MedType", "Name", "Note", "PieceUnit", "Updated", "UpdatedBy", "UsageWays" },
                values: new object[,]
                {
                    { -5, null, null, null, null, new Guid("00000000-0000-0000-0000-000000000000"), null, null, true, 0, "NGT", null, null, null, null, 0 },
                    { -4, null, null, null, null, new Guid("00000000-0000-0000-0000-000000000000"), null, null, true, 0, "Aminoven", null, null, null, null, 0 },
                    { -3, null, null, null, null, new Guid("00000000-0000-0000-0000-000000000000"), null, null, true, 0, "Buscopan", null, null, null, null, 0 },
                    { -2, null, null, null, null, new Guid("00000000-0000-0000-0000-000000000000"), null, null, true, 0, "Diovan", null, null, null, null, 0 },
                    { -1, null, null, null, null, new Guid("00000000-0000-0000-0000-000000000000"), null, null, true, 0, "Heparin", null, null, null, null, 0 }
                });

            migrationBuilder.InsertData(
                table: "Needles",
                columns: new[] { "Id", "Created", "CreatedBy", "IsActive", "Number", "Updated", "UpdatedBy" },
                values: new object[,]
                {
                    { -3, null, new Guid("00000000-0000-0000-0000-000000000000"), true, 17, null, null },
                    { -2, null, new Guid("00000000-0000-0000-0000-000000000000"), true, 16, null, null },
                    { -1, null, new Guid("00000000-0000-0000-0000-000000000000"), true, 15, null, null }
                });

            migrationBuilder.InsertData(
                table: "PatientHistoryItems",
                columns: new[] { "Id", "AllowOther", "Created", "CreatedBy", "DisplayName", "IsActive", "IsNumber", "IsYesNo", "Name", "Order", "TRT", "Updated", "UpdatedBy" },
                values: new object[,]
                {
                    { -8, false, null, new Guid("00000000-0000-0000-0000-000000000000"), "Drink", true, false, true, "drink", 8, 8, null, null },
                    { -7, false, null, new Guid("00000000-0000-0000-0000-000000000000"), "Smoke", true, false, true, "smoke", 7, 7, null, null },
                    { -6, false, null, new Guid("00000000-0000-0000-0000-000000000000"), "Income", true, true, false, "income", 6, 4, null, null },
                    { -5, true, null, new Guid("00000000-0000-0000-0000-000000000000"), "Career", true, false, false, "career", 5, 3, null, null },
                    { -4, true, null, new Guid("00000000-0000-0000-0000-000000000000"), "Education", true, false, false, "education", 4, 2, null, null },
                    { -3, false, null, new Guid("00000000-0000-0000-0000-000000000000"), "Marriage Status", true, false, false, "marriage", 3, 1, null, null },
                    { -2, false, null, new Guid("00000000-0000-0000-0000-000000000000"), "Weight", true, true, false, "weight", 2, 6, null, null },
                    { -1, false, null, new Guid("00000000-0000-0000-0000-000000000000"), "Height", true, true, false, "height", 1, 5, null, null }
                });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { new Guid("2bade702-40e6-4df4-890d-7f0c67d50207"), "296e852f-fbf5-4871-a12f-7f039db07929", "SuperAdministrator", "SUPERADMINISTRATOR" },
                    { new Guid("9453a296-a401-480b-bf81-bca7ab0f72a7"), "eeeef629-5a29-4607-98ef-9fa4a68ac365", "PN", "PN" },
                    { new Guid("a7c93a18-5c50-4e28-b02d-3b50cc3e17f1"), "44e7df22-327d-4e8d-9f87-2f59bc6b1cdc", "Doctor", "DOCTOR" },
                    { new Guid("baad21b1-6d08-4823-8cf3-0776a6266488"), "eab1f073-836a-42ad-9897-463792207f7d", "HeadNurse", "HEADNURSE" },
                    { new Guid("e958724f-6db8-4434-a804-f0277f02306a"), "faf97553-37bf-4eb7-bd25-721f0b51de9b", "Administrator", "ADMINISTRATOR" },
                    { new Guid("f903d36f-2531-4916-86bb-7b051aac7029"), "1f165522-607b-4ffa-9885-a8a341fb01d0", "Nurse", "NURSE" }
                });

            migrationBuilder.InsertData(
                table: "Status",
                columns: new[] { "Id", "Category", "Created", "CreatedBy", "IsActive", "Name", "Updated", "UpdatedBy" },
                values: new object[,]
                {
                    { -3, 2, null, new Guid("00000000-0000-0000-0000-000000000000"), true, "Full Recovery", null, null },
                    { -2, 0, null, new Guid("00000000-0000-0000-0000-000000000000"), true, "Deceased", null, null },
                    { -1, 1, null, new Guid("00000000-0000-0000-0000-000000000000"), true, "Hospital Transferred", null, null }
                });

            migrationBuilder.InsertData(
                table: "Underlyings",
                columns: new[] { "Id", "Created", "CreatedBy", "IsActive", "Name", "Updated", "UpdatedBy" },
                values: new object[,]
                {
                    { -7, null, new Guid("00000000-0000-0000-0000-000000000000"), true, "SLE", null, null },
                    { -6, null, new Guid("00000000-0000-0000-0000-000000000000"), true, "DLP", null, null },
                    { -5, null, new Guid("00000000-0000-0000-0000-000000000000"), true, "Stroke", null, null },
                    { -4, null, new Guid("00000000-0000-0000-0000-000000000000"), true, "CAD", null, null },
                    { -3, null, new Guid("00000000-0000-0000-0000-000000000000"), true, "CKD", null, null },
                    { -2, null, new Guid("00000000-0000-0000-0000-000000000000"), true, "HT", null, null },
                    { -1, null, new Guid("00000000-0000-0000-0000-000000000000"), true, "DM", null, null }
                });

            migrationBuilder.InsertData(
                table: "Units",
                columns: new[] { "Id", "Code", "Created", "CreatedBy", "HeadNurse", "IsActive", "Name", "Updated", "UpdatedBy" },
                values: new object[] { -1, null, null, new Guid("00000000-0000-0000-0000-000000000000"), null, true, "Unit 1", null, null });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "Email", "EmailConfirmed", "EmployeeId", "FirstName", "IsPartTime", "LastName", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "Signature", "TwoFactorEnabled", "UserName" },
                values: new object[] { new Guid("866dabc4-6501-44d2-a0e5-65da9c45a46e"), 0, "aadf088b-cf32-4817-8cfc-2c3ae4615fcb", null, true, null, "root", false, "admin", false, null, null, "ROOTADMIN", "AQAAAAEAACcQAAAAECMCAfv9MIbmyL/FfP+0ZaKk+COwT3IfxeviPM4NRZbhEvX+N0oAYWZsG0lPuKLasA==", null, true, "244fb073-9c47-4d58-ad27-5bd36aef059a", null, false, "rootadmin" });

            migrationBuilder.InsertData(
                table: "AspNetUserRoles",
                columns: new[] { "RoleId", "UserId" },
                values: new object[,]
                {
                    { new Guid("2bade702-40e6-4df4-890d-7f0c67d50207"), new Guid("866dabc4-6501-44d2-a0e5-65da9c45a46e") },
                    { new Guid("9453a296-a401-480b-bf81-bca7ab0f72a7"), new Guid("866dabc4-6501-44d2-a0e5-65da9c45a46e") },
                    { new Guid("a7c93a18-5c50-4e28-b02d-3b50cc3e17f1"), new Guid("866dabc4-6501-44d2-a0e5-65da9c45a46e") },
                    { new Guid("baad21b1-6d08-4823-8cf3-0776a6266488"), new Guid("866dabc4-6501-44d2-a0e5-65da9c45a46e") },
                    { new Guid("e958724f-6db8-4434-a804-f0277f02306a"), new Guid("866dabc4-6501-44d2-a0e5-65da9c45a46e") },
                    { new Guid("f903d36f-2531-4916-86bb-7b051aac7029"), new Guid("866dabc4-6501-44d2-a0e5-65da9c45a46e") }
                });

            migrationBuilder.InsertData(
                table: "AssessmentOptions",
                columns: new[] { "Id", "AssessmentId", "Created", "CreatedBy", "DisplayName", "IsActive", "IsDefault", "Name", "Note", "Order", "TextValue", "Updated", "UpdatedBy", "Value" },
                values: new object[,]
                {
                    { -99L, -18L, null, new Guid("00000000-0000-0000-0000-000000000000"), "Continue", true, false, "continue", null, 0, null, null, null, null },
                    { -98L, -18L, null, new Guid("00000000-0000-0000-0000-000000000000"), "Systolic", true, false, "systolic", null, 1, null, null, null, null },
                    { -97L, -19L, null, new Guid("00000000-0000-0000-0000-000000000000"), "Ok", true, false, "yes", null, 0, null, null, null, null },
                    { -96L, -21L, null, new Guid("00000000-0000-0000-0000-000000000000"), "No complication", true, false, "no", null, 0, null, null, null, null },
                    { -95L, -21L, null, new Guid("00000000-0000-0000-0000-000000000000"), "Hypo-tension", true, false, "hypo", null, 1, null, null, null, null },
                    { -94L, -21L, null, new Guid("00000000-0000-0000-0000-000000000000"), "Muscle cramp", true, false, "muscle", null, 2, null, null, null, null },
                    { -93L, -21L, null, new Guid("00000000-0000-0000-0000-000000000000"), "Headache", true, false, "head", null, 3, null, null, null, null },
                    { -92L, -21L, null, new Guid("00000000-0000-0000-0000-000000000000"), "Nausea/Vomit", true, false, "vomit", null, 4, null, null, null, null },
                    { -91L, -21L, null, new Guid("00000000-0000-0000-0000-000000000000"), "Fever", true, false, "fever", null, 5, null, null, null, null },
                    { -90L, -21L, null, new Guid("00000000-0000-0000-0000-000000000000"), "Hypertension", true, false, "hyper", null, 6, null, null, null, null },
                    { -89L, -21L, null, new Guid("00000000-0000-0000-0000-000000000000"), "Chest pain", true, false, "chest", null, 7, null, null, null, null },
                    { -88L, -21L, null, new Guid("00000000-0000-0000-0000-000000000000"), "Arrhythmia", true, false, "arr", null, 8, null, null, null, null },
                    { -87L, -21L, null, new Guid("00000000-0000-0000-0000-000000000000"), "Vascular access problem", true, false, "vascular", null, 9, null, null, null, null },
                    { -86L, -21L, null, new Guid("00000000-0000-0000-0000-000000000000"), "Others", true, false, "other", null, 10, null, null, null, null },
                    { -85L, -22L, null, new Guid("00000000-0000-0000-0000-000000000000"), "No complication", true, false, "no", null, 0, null, null, null, null },
                    { -84L, -22L, null, new Guid("00000000-0000-0000-0000-000000000000"), "Clotted dialyzer", true, false, "dialyzer", null, 1, null, null, null, null },
                    { -83L, -22L, null, new Guid("00000000-0000-0000-0000-000000000000"), "Clotted blood line", true, false, "bloodline", null, 2, null, null, null, null },
                    { -82L, -22L, null, new Guid("00000000-0000-0000-0000-000000000000"), "Machine problem", true, false, "machine", null, 3, null, null, null, null },
                    { -81L, -22L, null, new Guid("00000000-0000-0000-0000-000000000000"), "Blood leak", true, false, "bloodleak", null, 4, null, null, null, null },
                    { -80L, -23L, null, new Guid("00000000-0000-0000-0000-000000000000"), "Psychological support", true, false, "psycho", null, 0, null, null, null, null },
                    { -79L, -23L, null, new Guid("00000000-0000-0000-0000-000000000000"), "Trendelenburg position", true, false, "tren", null, 1, null, null, null, null },
                    { -78L, -23L, null, new Guid("00000000-0000-0000-0000-000000000000"), "Monitor vital signs", true, false, "vital", null, 2, null, null, null, null },
                    { -77L, -23L, null, new Guid("00000000-0000-0000-0000-000000000000"), "Pause ultrafiltration", true, false, "uf", null, 3, null, null, null, null },
                    { -76L, -23L, null, new Guid("00000000-0000-0000-0000-000000000000"), "Decrease dialysate temperature", true, false, "temp", null, 4, null, null, null, null },
                    { -75L, -23L, null, new Guid("00000000-0000-0000-0000-000000000000"), "Oxygen therapy", true, false, "oxygen", null, 5, null, null, null, null },
                    { -74L, -23L, null, new Guid("00000000-0000-0000-0000-000000000000"), "Hot compression", true, false, "hot", null, 6, null, null, null, null },
                    { -73L, -23L, null, new Guid("00000000-0000-0000-0000-000000000000"), "Strength exercise", true, false, "exercise", null, 7, null, null, null, null },
                    { -72L, -23L, null, new Guid("00000000-0000-0000-0000-000000000000"), "Cold compression", true, false, "cold", null, 8, null, null, null, null },
                    { -71L, -23L, null, new Guid("00000000-0000-0000-0000-000000000000"), "Aspirate precaution", true, false, "aspirate", null, 9, null, null, null, null },
                    { -70L, -23L, null, new Guid("00000000-0000-0000-0000-000000000000"), "Monitor EKG", true, false, "ekg", null, 10, null, null, null, null },
                    { -69L, -23L, null, new Guid("00000000-0000-0000-0000-000000000000"), "Decrease BFR", true, false, "bfr", null, 11, null, null, null, null },
                    { -68L, -23L, null, new Guid("00000000-0000-0000-0000-000000000000"), "Monitor access flow", true, false, "flow", null, 12, null, null, null, null },
                    { -67L, -23L, null, new Guid("00000000-0000-0000-0000-000000000000"), "Change dialyzer", true, false, "dialyzer", null, 13, null, null, null, null },
                    { -66L, -23L, null, new Guid("00000000-0000-0000-0000-000000000000"), "Change blood line", true, false, "bloodline", null, 14, null, null, null, null },
                    { -65L, -23L, null, new Guid("00000000-0000-0000-0000-000000000000"), "Notified physician", true, false, "notify", null, 15, null, null, null, null },
                    { -64L, -23L, null, new Guid("00000000-0000-0000-0000-000000000000"), "Others", true, false, "other", null, 16, null, null, null, null },
                    { -63L, -24L, null, new Guid("00000000-0000-0000-0000-000000000000"), "Nutrition", true, false, "nutrition", null, 0, null, null, null, null },
                    { -62L, -24L, null, new Guid("00000000-0000-0000-0000-000000000000"), "Vascular access", true, false, "vascular", null, 1, null, null, null, null },
                    { -61L, -24L, null, new Guid("00000000-0000-0000-0000-000000000000"), "Exercise", true, false, "exercise", null, 2, null, null, null, null },
                    { -60L, -24L, null, new Guid("00000000-0000-0000-0000-000000000000"), "Personal hygiene", true, false, "hygiene", null, 3, null, null, null, null },
                    { -59L, -24L, null, new Guid("00000000-0000-0000-0000-000000000000"), "Medication", true, false, "medication", null, 4, null, null, null, null },
                    { -58L, -24L, null, new Guid("00000000-0000-0000-0000-000000000000"), "Fluid control", true, false, "fluid", null, 5, null, null, null, null },
                    { -57L, -24L, null, new Guid("00000000-0000-0000-0000-000000000000"), "KT preparation", true, false, "kt", null, 6, null, null, null, null }
                });

            migrationBuilder.InsertData(
                table: "PatientChoice",
                columns: new[] { "Id", "PatientHistoryItemId", "NumberValue", "Text" },
                values: new object[,]
                {
                    { -87, -5, null, "State Enterprise Employee" },
                    { -86, -5, null, "Business Owner" },
                    { -85, -5, null, "Goverment Officer" },
                    { -84, -5, null, "Employee" },
                    { -83, -5, null, "Freelance" },
                    { -96, -4, null, "Primary School" },
                    { -95, -4, null, "Middle School" },
                    { -94, -4, null, "High School" },
                    { -93, -4, null, "Vocational Certificate" },
                    { -92, -4, null, "High Vocational Certificate" },
                    { -91, -4, null, "Diploma" },
                    { -90, -4, null, "Bachelor's Degree" },
                    { -89, -4, null, "Master's Degree" },
                    { -88, -4, null, "Doctor's Degree/ Ph.D." },
                    { -99, -3, null, "Married" },
                    { -98, -3, null, "Divorced" },
                    { -97, -3, null, "Single" }
                });

            migrationBuilder.CreateIndex(
                name: "Index_Admission_Underlying",
                table: "AdmissionUnderlyings",
                column: "AdmissionId");

            migrationBuilder.CreateIndex(
                name: "IX_AdmissionUnderlyings_UnderlyingId",
                table: "AdmissionUnderlyings",
                column: "UnderlyingId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_AssessmentItems_AssessmentId",
                table: "AssessmentItems",
                column: "AssessmentId");

            migrationBuilder.CreateIndex(
                name: "IX_AssessmentItems_HemosheetId",
                table: "AssessmentItems",
                column: "HemosheetId");

            migrationBuilder.CreateIndex(
                name: "IX_AssessmentOptions_AssessmentId",
                table: "AssessmentOptions",
                column: "AssessmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Assessments_GroupId",
                table: "Assessments",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_DialysisRecordAssessmentItems_AssessmentId",
                table: "DialysisRecordAssessmentItems",
                column: "AssessmentId");

            migrationBuilder.CreateIndex(
                name: "IX_DialysisRecordAssessmentItems_DialysisRecordId",
                table: "DialysisRecordAssessmentItems",
                column: "DialysisRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_DialyzerStock_ItemId",
                table: "DialyzerStock",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_EquipmentStock_ItemId",
                table: "EquipmentStock",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ExecutionRecords_HemodialysisId",
                table: "ExecutionRecords",
                column: "HemodialysisId");

            migrationBuilder.CreateIndex(
                name: "IX_ExecutionRecords_PrescriptionId",
                table: "ExecutionRecords",
                column: "PrescriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_HemodialysisRecords_DialysisPrescriptionId",
                table: "HemodialysisRecords",
                column: "DialysisPrescriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_HemodialysisRecords_PostVitalsign_HemodialysisRecordId",
                table: "HemodialysisRecords_PostVitalsign",
                column: "HemodialysisRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_HemodialysisRecords_PreVitalsign_HemodialysisRecordId",
                table: "HemodialysisRecords_PreVitalsign",
                column: "HemodialysisRecordId");

            migrationBuilder.CreateIndex(
                name: "Index_HemoId",
                table: "HemoNotes",
                column: "HemoId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LabExamItems_Name",
                table: "LabExamItems",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LabExams_LabItemId",
                table: "LabExams",
                column: "LabItemId");

            migrationBuilder.CreateIndex(
                name: "IX_LabExams_PatientId",
                table: "LabExams",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_MedicalSupplyStock_ItemId",
                table: "MedicalSupplyStock",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_MedicineHistories_MedicineId",
                table: "MedicineHistories",
                column: "MedicineId");

            migrationBuilder.CreateIndex(
                name: "IX_MedicinePrescriptions_MedicineId",
                table: "MedicinePrescriptions",
                column: "MedicineId");

            migrationBuilder.CreateIndex(
                name: "IX_Medicines_CategoryId",
                table: "Medicines",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_MedicineStock_ItemId",
                table: "MedicineStock",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "Index_PatientId_Allergy",
                table: "PatientMedicine",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "Index_Unique",
                table: "PatientMedicine",
                columns: new[] { "Type", "PatientId", "MedicineId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PatientMedicine_MedicineId",
                table: "PatientMedicine",
                column: "MedicineId");

            migrationBuilder.CreateIndex(
                name: "Index_Name",
                table: "Patients",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_ProgressNotes_HemodialysisId",
                table: "ProgressNotes",
                column: "HemodialysisId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "Roles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Schedules_PatientId",
                table: "Schedules",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_Schedules_SectionId",
                table: "Schedules",
                column: "SectionId");

            migrationBuilder.CreateIndex(
                name: "IX_Sections_UnitId",
                table: "Sections",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_SectionSlotPatients_SectionId",
                table: "SectionSlotPatients",
                column: "SectionId");

            migrationBuilder.CreateIndex(
                name: "IX_ShiftInchargeSection_SectionId",
                table: "ShiftInchargeSection",
                column: "SectionId");

            migrationBuilder.CreateIndex(
                name: "IX_ShiftMeta_ScheduleMetaId",
                table: "ShiftMeta",
                column: "ScheduleMetaId");

            migrationBuilder.CreateIndex(
                name: "IX_ShiftSlots_ShiftMetaId",
                table: "ShiftSlots",
                column: "ShiftMetaId");

            migrationBuilder.CreateIndex(
                name: "shift_slot_unique_keys",
                table: "ShiftSlots",
                columns: new[] { "UserId", "Date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tags_PatientId",
                table: "Tags",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "Users",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "Users",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "user_shift_unique_keys",
                table: "UserShifts",
                columns: new[] { "UserId", "Month" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserUnits_UserId",
                table: "UserUnits",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AdmissionUnderlyings");

            migrationBuilder.DropTable(
                name: "Anticoagulants");

            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "AssessmentItems");

            migrationBuilder.DropTable(
                name: "AssessmentOptions");

            migrationBuilder.DropTable(
                name: "AvShuntIssueTreatments");

            migrationBuilder.DropTable(
                name: "AvShunts");

            migrationBuilder.DropTable(
                name: "CauseOfDeath");

            migrationBuilder.DropTable(
                name: "Dialysates");

            migrationBuilder.DropTable(
                name: "DialysisRecordAssessmentItems");

            migrationBuilder.DropTable(
                name: "DialyzerStock");

            migrationBuilder.DropTable(
                name: "DoctorRecords");

            migrationBuilder.DropTable(
                name: "EquipmentStock");

            migrationBuilder.DropTable(
                name: "ExecutionRecords");

            migrationBuilder.DropTable(
                name: "Files");

            migrationBuilder.DropTable(
                name: "HemodialysisRecords_PostVitalsign");

            migrationBuilder.DropTable(
                name: "HemodialysisRecords_PreVitalsign");

            migrationBuilder.DropTable(
                name: "HemoNotes");

            migrationBuilder.DropTable(
                name: "LabExams");

            migrationBuilder.DropTable(
                name: "LabHemosheets");

            migrationBuilder.DropTable(
                name: "MedicalSupplyStock");

            migrationBuilder.DropTable(
                name: "MedicineHistories");

            migrationBuilder.DropTable(
                name: "MedicineStock");

            migrationBuilder.DropTable(
                name: "Needles");

            migrationBuilder.DropTable(
                name: "NurseRecords");

            migrationBuilder.DropTable(
                name: "PatientChoice");

            migrationBuilder.DropTable(
                name: "PatientHistories");

            migrationBuilder.DropTable(
                name: "PatientMedicine");

            migrationBuilder.DropTable(
                name: "ProgressNotes");

            migrationBuilder.DropTable(
                name: "Schedules");

            migrationBuilder.DropTable(
                name: "SectionSlotPatients");

            migrationBuilder.DropTable(
                name: "ShiftInchargeSection");

            migrationBuilder.DropTable(
                name: "ShiftSlots");

            migrationBuilder.DropTable(
                name: "Status");

            migrationBuilder.DropTable(
                name: "Tags");

            migrationBuilder.DropTable(
                name: "TempSections");

            migrationBuilder.DropTable(
                name: "UserShifts");

            migrationBuilder.DropTable(
                name: "UserUnits");

            migrationBuilder.DropTable(
                name: "Wards");

            migrationBuilder.DropTable(
                name: "Admissions");

            migrationBuilder.DropTable(
                name: "Underlyings");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "DialysisRecords");

            migrationBuilder.DropTable(
                name: "Assessments");

            migrationBuilder.DropTable(
                name: "Dialyzers");

            migrationBuilder.DropTable(
                name: "Equipments");

            migrationBuilder.DropTable(
                name: "MedicinePrescriptions");

            migrationBuilder.DropTable(
                name: "LabExamItems");

            migrationBuilder.DropTable(
                name: "MedicalSupplies");

            migrationBuilder.DropTable(
                name: "StockItems");

            migrationBuilder.DropTable(
                name: "PatientHistoryItems");

            migrationBuilder.DropTable(
                name: "HemodialysisRecords");

            migrationBuilder.DropTable(
                name: "Sections");

            migrationBuilder.DropTable(
                name: "ShiftIncharges");

            migrationBuilder.DropTable(
                name: "ShiftMeta");

            migrationBuilder.DropTable(
                name: "Patients");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "AssessmentGroups");

            migrationBuilder.DropTable(
                name: "Medicines");

            migrationBuilder.DropTable(
                name: "DialysisPrescriptions");

            migrationBuilder.DropTable(
                name: "Units");

            migrationBuilder.DropTable(
                name: "ScheduleMeta");

            migrationBuilder.DropTable(
                name: "MedCategories");
        }
    }
}
