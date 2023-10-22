using AutoMapper;
using MessagePack;
using Microsoft.Extensions.Configuration;
using SkiaSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Telerik.Reporting.Drawing;
using Telerik.Reporting.Expressions;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Models.Enums;
using SKSvg = SkiaSharp.Extended.Svg.SKSvg;
using Unit = Wasenshi.HemoDialysisPro.Models.Unit;

namespace Wasenshi.HemoDialysisPro.Report.Models
{
    public class HemosheetData
    {
        [IgnoreMember]
        private readonly IMapper mapper;
        [IgnoreMember]
        private readonly IConfiguration config;
        [IgnoreMember]
        private IUserResolver userResolver { get; }

        public HemosheetData(IUserResolver userResolver, IMapper mapper, IConfiguration config)
        {
            this.mapper = mapper;
            this.config = config;
            this.userResolver = userResolver;
            Patient = new PatientData(userResolver, config);
            Dehydration = new DehydrationData(this);
        }

        [SerializationConstructor]
        public HemosheetData()
        {
            // for serialization
        }

        public string Logo { get; set; }
        public string DoctorSignature { get; set; }
        public bool IsConsent { get; set; }

        public Unit Unit { get; set; }

        public int? TreatmentNo { get; set; }
        public ICollection<FlushRecord> Flush => ExecutionRecords.Where(r => r.Type == ExecutionType.NSSFlush).OrderBy(x => x.Timestamp).Cast<FlushRecord>().ToArray();

        public Guid Id { get; set; }
        public string Ward { get; set; }
        public string Bed { get; set; }
        public string Admission { get; set; }
        public DateTime? CycleStartTime { get; set; }
        public DateTime? CycleEndTime { get; set; }
        public DateTime? CompletedTime { get; set; }
        public Guid? ProofReader { get; set; }
        public Guid CreatedBy { get; set; }

        public bool IsICU { get; set; }
        public DialysisType Type { get; set; }

        public string ProofReaderName => ProofReader.HasValue ? userResolver.GetName(ProofReader.Value) : null;
        public string ProofReaderEmployeeId => ProofReader.HasValue ? userResolver.GetEmployeeId(ProofReader.Value) : null;
        public string CreatorName => userResolver.GetName(CreatedBy);
        public string CreatorEmployeeId => userResolver.GetEmployeeId(CreatedBy);

        public bool AcNotUsed { get; set; }
        public string ReasonForRefraining { get; set; }

        // Planning (actual amount will be calculated from dialysis record)
        public float? FlushNSS { get; set; } // ml
        public int? FlushNSSInterval { get; set; } // interval in minutes (min)
        public int? FlushTimes { get; set; } // Flush NSS multiplied by this = Total amount (ml)
        public float? FlushNSSTotal => FlushNSS * FlushTimes;

        public bool IsAcNotUsed => AcNotUsed || (string.IsNullOrWhiteSpace(DialysisPrescription?.Anticoagulant) && !HasAnyAcInfo);
        public bool IsUnknownAc => string.IsNullOrWhiteSpace(DialysisPrescription?.Anticoagulant) && HasAnyAcInfo;
        public bool HasAnyAcInfo => (DialysisPrescription?.InitialAmountMl.HasValue ?? false) || (DialysisPrescription?.InitialAmount.HasValue ?? false)
                                   || (DialysisPrescription?.MaintainAmountMl.HasValue ?? false) || (DialysisPrescription?.MaintainAmount.HasValue ?? false)
                                   || (DialysisPrescription?.AcPerSessionMl.HasValue ?? false) || (DialysisPrescription?.AcPerSession.HasValue ?? false);
        public string ReasonForRefrainMap => DialysisPrescription?.ReasonForRefraining ?? ReasonForRefraining;

        public string NursesInShift { get; set; }

        public PatientData Patient { get; set; }
        public Admission AdmissionInfo { get; set; }

        public Assessments Assessments { get; set; } = new Assessments();
        public DehydrationData Dehydration { get; set; }
        public AVShuntRecord AvShunt { get; set; } = new AVShuntRecord();
        public DialysisPrescriptionData DialysisPrescription { get; set; }
        public DialyzerRecord Dialyzer { get; set; } = new DialyzerRecord();

        public ICollection<VitalSignRecord> PreVitalsign { get; set; } = new List<VitalSignRecord>();
        public ICollection<VitalSignRecord> PostVitalsign { get; set; } = new List<VitalSignRecord>();

        public VitalSignRecord PreVitalFinal => PreVitalsign?.OrderBy(x => x.Timestamp).FirstOrDefault();
        public VitalSignRecord PostVitalFinal => PostVitalsign?.OrderBy(x => x.Timestamp).FirstOrDefault();

        // Records
        public ICollection<DialysisRecordData> DialysisRecords { get; set; } = new List<DialysisRecordData>();
        public ICollection<NurseRecordData> NurseRecords { get; set; } = new List<NurseRecordData>();
        public ICollection<DoctorRecordData> DoctorRecords { get; set; } = new List<DoctorRecordData>();
        public ICollection<ExecutionRecord> ExecutionRecords { get; set; } = new List<ExecutionRecord>();

        public ICollection<MedicineRecordData> MedicineRecords => ExecutionRecords.Where(x => x.Type == ExecutionType.Medicine).Select(x => mapper.Map<MedicineRecordData>(x)).ToList();

        public LabData Labs { get; set; } = new LabData();

        public ICollection<ProgressNoteData> ProgressNotes { get; set; }

        public Dictionary<string, object> Extras { get; set; } = new Dictionary<string, object>();

        // Util short-cut
        public float? Ktv => LastRecord?.Ktv;
        public float? URR => LastRecord?.URR;
        public float? PRR => LastRecord?.PRR;
        public float? Recir => LastRecord?.RecirculationRate;
        public float? DBV => LastRecord?.DBV;
        public float? SubVol => LastRecord?.SAV;
        public DialysisRecordData LastRecord => DialysisRecords.LastOrDefault(x => x.Created.HasValue);
        public DialysisRecordData FirstRecord => DialysisRecords.FirstOrDefault(x => x.Created.HasValue);


        // =========================== Functions ===================================

        public string GetUserFullname(Guid userId)
        {
            return userResolver.GetName(userId);
        }

        [Function(Category = "Hemo", Namespace = "Hemo")]
        public static string GetName(Guid userId, object hemosheet)
        {
            if (hemosheet is HemosheetData data)
            {
                return data.GetUserFullname(userId);
            }

            return null;
        }

        public string GetUserEmployeeId(Guid userId)
        {
            return userResolver.GetEmployeeId(userId);
        }

        [Function(Category = "Hemo", Namespace = "Hemo")]
        public static string GetEmployeeId(Guid userId, object hemosheet)
        {
            if (hemosheet is HemosheetData data)
            {
                return data.GetUserEmployeeId(userId);
            }

            return null;
        }


        [Function(Category = "Hemo", Namespace = "Hemo")]
        public static AssessmentData Get(Dictionary<string, AssessmentData> list, string name)
        {
            return (AssessmentData)GetItem(list, name);
        }

        [Function(Category = "Hemo", Namespace = "Hemo")]
        public static IMetaData GetSelect(AssessmentData data, string name)
        {
            return data?.Selected.FirstOrDefault(x => x.Name == name);
        }

        [Function(Category = "Hemo", Namespace = "Hemo")]
        public static IMetaData GetSelect(AssessmentData data)
        {
            return data?.Selected.FirstOrDefault();
        }

        [Function(Category = "Hemo", Namespace = "Hemo")]
        public static object GetItem(IDictionary list, string name)
        {
            if (list == null) return null;
            if (!list.Contains(name))
            {
                return null;
            }
            return list[name];
        }

        [Function(Category = "Hemo", Namespace = "Hemo")]
        public static string[] Map<T>(IEnumerable<T> list, string propertyName)
        {
            throw new NotImplementedException();
        }

        [Function(Category = "Hemo", Namespace = "Hemo")]
        public static LabExam GetLab(LabData labs, string name)
        {
            return labs.GetLab(name);
        }

        [Function(Category = "Hemosheet", Namespace = "Hemosheet")]
        public static Telerik.Reporting.Drawing.Unit GetLogoRatioWidth(SizeU size, string logoFile)
        {
            if (string.IsNullOrWhiteSpace(logoFile))
            {
                return size.Width;
            }

            float h = size.Height.Value;
            float w = size.Width.Value;

            var logo = GetImageSize(logoFile);
            double targetRatio = (double)logo.width / logo.height;
            double currentRatio = w / h;

            double targetW = w * targetRatio / currentRatio;

            Telerik.Reporting.Drawing.Unit unit = new(targetW, size.Width.Type);

            return unit;
        }

        [Function(Category = "Hemosheet", Namespace = "Hemosheet")]
        public static Telerik.Reporting.Drawing.PointU GetLogoRightLocation(SizeU size, string logoFile)
        {
            if (string.IsNullOrWhiteSpace(logoFile))
            {
                return new PointU(Telerik.Reporting.Drawing.Unit.Zero, Telerik.Reporting.Drawing.Unit.Zero);
            }

            var targetWUnit = GetLogoRatioWidth(size, logoFile);
            var diff = size.Width - targetWUnit;

            var result = new PointU(diff, Telerik.Reporting.Drawing.Unit.Zero);
            return result;
        }


        public static (float width, float height) GetImageSize(string file)
        {
            if (System.IO.Path.GetExtension(file) == ".svg")
            {
                SKSvg svg = new();
                svg.Load(file);
                return (svg.CanvasSize.Width, svg.CanvasSize.Height);
            }
            else
            {
                var image = SKBitmap.Decode(Helper.Image(file));
                return (image.Width, image.Height);
            }
        }
    }
}
