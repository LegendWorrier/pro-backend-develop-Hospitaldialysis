using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ServiceStack.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using Telerik.Reporting;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Models.Enums;
using Wasenshi.HemoDialysisPro.Models.Settings;
using Wasenshi.HemoDialysisPro.Report.Models;
using Wasenshi.HemoDialysisPro.Repositories.Interfaces;
using Wasenshi.HemoDialysisPro.Repositories.Interfaces.Base;
using Wasenshi.HemoDialysisPro.Repository.Core.Interfaces.Base;
using Wasenshi.HemoDialysisPro.Services;
using Wasenshi.HemoDialysisPro.Services.Core.BusinessLogic;
using Wasenshi.HemoDialysisPro.Utils;
using Wasenshi.HemoDialysisPro.Share;
using Wasenshi.HemoDialysisPro.Services.Core.Interfaces;
using Wasenshi.HemoDialysisPro.PluginBase;
using Wasenshi.HemoDialysisPro.PluginIntegrate;
using System.Threading.Tasks;

namespace Wasenshi.HemoDialysisPro.Report.DocumentLogics
{
    public class HemosheetResolver : DocResolverBase
    {
        private readonly IMapper mapper;
        private readonly IHemoUnitOfWork hemoUnit;
        private readonly IShiftUnitOfWork shiftUnit;
        private readonly IUserInfoService userInfoService;
        private readonly IAdmissionRepository admissionRepo;
        private readonly ILabExamRepository labExamRepo;
        private readonly IAssessmentRepository assessmentRepo;
        private readonly IAssessmentItemRepository assessmentItemRepo;
        private readonly IRepository<AssessmentGroup, int> assessmentGroupRepo;
        private readonly IDialysisRecordRepository dialysisRecordRepo;
        private readonly IAvShuntRepository avShuntRepo;
        private readonly IRepository<NurseRecord, Guid> nurseRecordRepo;
        private readonly IRepository<DoctorRecord, Guid> doctorRecordRepo;
        private readonly IRepository<ProgressNote, Guid> progressNoteRepo;
        private readonly IRepository<Unit, int> unitRepo;
        private readonly IExecutionRecordRepository executionRecordRepo;
        private readonly IEnumerable<IDocumentHandler> documentPlugins;
        private readonly IRedisClient redis;
        private readonly IContextAdapter context;

        public HemosheetResolver(
            ILogger<HemosheetResolver> logger,
            IUserResolver userResolver,
            IFileRepository fileRepo,
            IConfiguration config,
            IMapper mapper,
            IHemoUnitOfWork hemoUnit,
            IShiftUnitOfWork shiftUnit,
            IUserInfoService userInfoService,
            IAdmissionRepository admissionRepo,
            ILabExamRepository labExamRepo,
            IAssessmentRepository assessmentRepo,
            IAssessmentItemRepository assessmentItemRepo,
            IRepository<AssessmentGroup, int> assessmentGroupRepo,
            IDialysisRecordRepository dialysisRecordRepo,
            IAvShuntRepository avShuntRepo,
            IRepository<NurseRecord, Guid> nurseRecordRepo,
            IRepository<DoctorRecord, Guid> doctorRecordRepo,
            IRepository<ProgressNote, Guid> progressNoteRepo,
            IRepository<Unit, int> unitRepo,
            IExecutionRecordRepository executionRecordRepo,
            IEnumerable<IDocumentHandler> documentPlugins,
            IRedisClient redis,
            IContextAdapter context
            ) : base(logger, userResolver, fileRepo, config)
        {
            this.mapper = mapper;
            this.hemoUnit = hemoUnit;
            this.shiftUnit = shiftUnit;
            this.userInfoService = userInfoService;
            this.admissionRepo = admissionRepo;
            this.labExamRepo = labExamRepo;
            this.assessmentRepo = assessmentRepo;
            this.assessmentItemRepo = assessmentItemRepo;
            this.assessmentGroupRepo = assessmentGroupRepo;
            this.dialysisRecordRepo = dialysisRecordRepo;
            this.avShuntRepo = avShuntRepo;
            this.nurseRecordRepo = nurseRecordRepo;
            this.doctorRecordRepo = doctorRecordRepo;
            this.progressNoteRepo = progressNoteRepo;
            this.unitRepo = unitRepo;
            this.executionRecordRepo = executionRecordRepo;
            this.documentPlugins = documentPlugins;
            this.redis = redis;
            this.context = context;
        }

        public override object GetData(object data)
        {
            var result = new HemosheetData(userResolver, mapper, config);
            mapper.Map(data as HemosheetData, result);
            return result;
        }

        public override async Task<object> PrepareData(IDictionary<string, object> parameters)
        {
            var hemoId = Guid.Parse((string)parameters["hemoId"]);
            return await PrepareHemosheetData(hemoId);
        }

        public override async Task<object> UpdateData(object prevData)
        {
            var hemoId = ((HemosheetData)prevData).Id;
            return await PrepareHemosheetData(hemoId);
        }

        public override void ExtraSetup(InstanceReportSource report)
        {
            // extra
            _ = bool.TryParse(config["Reports:Hemosheet:HasReassessment"], out bool hasReassessment);
            report.Parameters.Add("reassessment", hasReassessment);
            Align logoAlign = redis.Get<Align>(Common.LOGO_ALIGN);
            logger.LogInformation("logo align: {Align}", logoAlign);
            report.Parameters.Add("logo_align", logoAlign.ToString());
        }

        // ===============================================

        private async Task<HemosheetData> PrepareHemosheetData(Guid hemoId)
        {
            try
            {
                var hemosheet = hemoUnit.HemoRecord.Get(hemoId);
                var patient = hemoUnit.Patient.Get(hemosheet.PatientId);

                // med history and allergy
                patient.Allergy = hemoUnit.Patient.Allergies.Where(x => x.PatientId == patient.Id)
                    .Select(x => new HemoDialysisPro.Models.MappingModels.Allergy { Medicine = x.Medicine }).ToList();

                var result = new HemosheetData(userResolver, mapper, config);
                result = mapper.Map(hemosheet, result);
                result.Patient = mapper.Map(patient, result.Patient);
                result.Unit = unitRepo.GetAll(false).FirstOrDefault(x => x.Id == result.Patient.UnitId);

                result.Logo = GetLogo();
                result.TreatmentNo = hemosheet.TreatmentNo ??
                    (patient.DialysisInfo?.AccumulatedTreatmentTimes ?? 0) + hemoUnit.HemoRecord.GetAll(false).Count(x => x.PatientId == hemosheet.PatientId);
                DateTime targetDate = (hemosheet.CompletedTime ?? hemosheet.Created.Value).AsUtcDate();
                var admit = admissionRepo.GetAll(false)
                    .FirstOrDefault(x => x.PatientId == patient.Id && x.Admit <= targetDate && (x.Discharged == null || x.Discharged.Value >= targetDate));
                if (admit != null)
                {
                    result.AdmissionInfo = mapper.Map<AdmissionData>(admit);
                }

                var targetDay = TimeZoneInfo.ConvertTime(new DateTimeOffset(hemosheet.CompletedTime ?? hemosheet.Created.Value, TimeSpan.Zero), tz);
                var lowerLimit = targetDay.ToUtcDate();
                var upperLimit = lowerLimit.AddDays(1);

                var labList = context.Context.LabHemosheets;
                var labsIntermediate = labList.Join(
                        labExamRepo.GetAll(true).Where(x => x.PatientId == patient.Id),
                        o => o.LabItemId, i => i.LabItemId, (l, i) => new { Lab = l, Data = i })
                    .Where(x => (x.Lab.OnlyOnDate && x.Data.EntryTime >= lowerLimit && x.Data.EntryTime < upperLimit) || !x.Lab.OnlyOnDate)
                    .GroupBy(x => x.Lab.LabItemId)
                    .Select(x => x.OrderByDescending(i => i.Data.EntryTime).First())
                    .ToList();

                result.Labs.AddRange(labsIntermediate.Select( x => KeyValuePair.Create(x.Lab.Name, x.Data)));

                if (result.Dialyzer.UseNo < 1)
                {
                    result.Dialyzer.UseNo = 1;
                }

                result.IsConsent = hemosheet.DoctorConsent;
                if (hemosheet.DoctorConsent && (hemosheet.DoctorId.HasValue || patient.DoctorId.HasValue))
                {
                    result.DoctorSignature = GetSignature(hemosheet.DoctorId ?? patient.DoctorId.Value);
                }

                if (hemosheet.Dehydration.LastPostWeight == 0)
                {
                    DateTime target = hemosheet.Created.Value.AsUtcDate();
                    var lastCompleted = hemoUnit.HemoRecord.GetAll(false)
                        .Where(x => x.PatientId == hemosheet.PatientId && x.Created < target)
                        .OrderByDescending(x => x.Created)
                        .AsSingleQuery()
                        .FirstOrDefault();

                    result.Dehydration.LastPostWeight = lastCompleted?.Dehydration.PostWeight() ?? 0;
                }
                if (!result.Dehydration.BloodTransfusion.HasValue)
                {
                    result.Dehydration.BloodTransfusion = result.DialysisPrescription?.BloodTransfusion ?? 0;
                }
                if (!result.Dehydration.ExtraFluid.HasValue)
                {
                    result.Dehydration.ExtraFluid = result.DialysisPrescription?.ExtraFluid ?? 0;
                }
                if (result.AvShunt?.AVShuntId.HasValue ?? false)
                {
                    result.AvShunt.AVShunt = avShuntRepo.Get(result.AvShunt.AVShuntId.Value);
                }

                System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
                sw.Start();
                var metaDataAssessments = assessmentRepo.GetAll().ToList();
                result.Assessments.Metadata = metaDataAssessments.ToDictionary(a => a.Name, a => (IMetaData)new MetaData
                {
                    DisplayName = a.DisplayName,
                    Name = a.Name
                });
                result.Assessments.OptionMetadata = metaDataAssessments
                    .Where(x => x.OptionType != OptionTypes.Text && x.OptionType != OptionTypes.Number)
                    .SelectMany(x => x.OptionsList.Where(o => !string.IsNullOrEmpty(o.Name)).Select(o => new { AssessmentName = x.Name, OptionDetail = o }))
                    .ToDictionary(a => $"{a.AssessmentName}:{a.OptionDetail.Name}", a => (IMetaData)new MetaData
                    {
                        DisplayName = a.OptionDetail.DisplayName,
                        Name = a.OptionDetail.Name
                    });
                // groups
                var metaGroups = assessmentGroupRepo.GetAll(false).ToList();
                result.Assessments.Metadata = new Dictionary<string, IMetaData>(result.Assessments.Metadata
                    .Concat(metaGroups.ToDictionary(a => $"grp:{a.Name}", a => (IMetaData)new MetaData
                    {
                        Name = a.Name,
                        DisplayName = a.DisplayName,
                    })));
                var assessmentItems = assessmentItemRepo.GetAll().Where(x => x.HemosheetId == hemoId).ToList();
                var assessmentData = metaDataAssessments.Join(assessmentItems,
                    a => a.Id,
                    i => i.AssessmentId,
                    (assessment, item) =>
                    {
                        var selected = assessment.OptionsList.Where(n => item.Selected?.Contains(n.Id) ?? false).Select(a =>
                            (IMetaData)new MetaData
                            {
                                DisplayName = a.DisplayName,
                                Name = a.Name
                            });
                        if (assessment.HasOther && (item.Selected?.Contains(0) ?? false))
                        {
                            selected = selected.Concat(new[] { (IMetaData)new MetaData { DisplayName = "Other", Name = "other" } });
                        }
                        var map = mapper.Map<AssessmentData>(item);
                        map.Selected = selected.ToArray();

                        return new
                        {
                            Meta = assessment,
                            Data = map
                        };
                    }).ToList();

                var pre = assessmentData.Where(x => x.Meta.Type == AssessmentTypes.Pre && !x.Data.IsReassessment);
                var re = assessmentData.Where(x => x.Meta.Type == AssessmentTypes.Pre && x.Data.IsReassessment);
                var post = assessmentData.Where(x => x.Meta.Type == AssessmentTypes.Post);
                var other = assessmentData.Where(x => x.Meta.Type == AssessmentTypes.Other);

                result.Assessments.Other = other.ToDictionary(x => x.Meta.Name, x => x.Data);
                result.Assessments.Post = post.ToDictionary(x => x.Meta.Name, x => x.Data);
                result.Assessments.Pre = pre.ToDictionary(x => x.Meta.Name, x => x.Data);
                result.Assessments.Re = re.ToDictionary(x => x.Meta.Name, x => x.Data);
                sw.Stop();
                logger.LogDebug($"[REPORT]: Time elasped for [Map assessment dict]: {sw.ElapsedMilliseconds} milliseconds");
                Console.WriteLine($"[REPORT]: Time elasped for [Map assessment dict]: {sw.ElapsedMilliseconds} milliseconds");

                var executionRecords = executionRecordRepo.GetAll().Where(x => x.HemodialysisId == hemoId && x.IsExecuted);
                result.ExecutionRecords = executionRecords.OrderBy(x => x.Timestamp).ToList();

                var dialysisRecords = dialysisRecordRepo.GetAll().Where(x => x.HemodialysisId == hemoId && !x.IsFromMachine).OrderBy(x => x.Timestamp).ToList();
                result.DialysisRecords = mapper.Map<List<DialysisRecordData>>(dialysisRecords);

                foreach (var item in result.DialysisRecords)
                {
                    if  ( item.AssessmentItems?.Any() ?? false)
                    {
                        var assessments = metaDataAssessments.Join(item.AssessmentItems,
                            a => a.Id,
                            i => i.AssessmentId,
                            (assessment, item) =>
                            {
                                var selected = assessment.OptionsList.Where(n => item.Selected?.Contains(n.Id) ?? false).Select(a =>
                                    (IMetaData)new MetaData
                                    {
                                        DisplayName = a.DisplayName,
                                        Name = a.Name
                                    });
                                if (assessment.HasOther && (item.Selected?.Contains(0) ?? false))
                                {
                                    selected = selected.Concat(new[] { (IMetaData)new MetaData { DisplayName = "Other", Name = "other" } });
                                }
                                var map = mapper.Map<AssessmentData>(item);
                                map.Selected = selected.ToArray();

                                return new
                                {
                                    Meta = assessment,
                                    Data = map
                                };
                            }).ToList();
                        item.AssessmentDict = assessments.ToDictionary(x => x.Meta.Name, x => x.Data);
                    }
                }

                var nurseRecords = nurseRecordRepo.GetAll().Where(x => x.HemodialysisId == hemoId).OrderBy(x => x.Timestamp).ToList();
                result.NurseRecords = mapper.Map<List<NurseRecordData>>(nurseRecords);
                var doctorRecords = doctorRecordRepo.GetAll().Where(x => x.HemodialysisId == hemoId).OrderBy(x => x.Timestamp).ToList();
                result.DoctorRecords = mapper.Map<List<DoctorRecordData>>(doctorRecords);

                var progressNotes = progressNoteRepo.GetAll().Where(x => x.HemodialysisId == hemoId).OrderBy(x => x.Order).ToList();
                result.ProgressNotes = mapper.Map<List<ProgressNoteData>>(progressNotes);

                // ================= Fixed Lines ======================
                if (!int.TryParse(config["Reports:Hemosheet:FixedLines"], out int fixedLines))
                {
                    fixedLines = 8;
                }
                if (result.DialysisRecords.Count < fixedLines)
                {
                    int doubleLinesCount = result.DialysisRecords.Count(x => x.Note != null && x.Note.Length > 40);
                    int fillingEmptyLine = fixedLines - (result.DialysisRecords.Count + doubleLinesCount);

                    for (int i = 0; i < fillingEmptyLine; i++)
                    {
                        result.DialysisRecords.Add(new DialysisRecordData());
                    }
                }
                if (!int.TryParse(config["Reports:Hemosheet:FixedLines:Nurse"], out int fixedLinesNurse))
                {
                    fixedLinesNurse = 0;
                }
                if (result.NurseRecords.Count < fixedLinesNurse)
                {
                    int doubleLinesCount = result.NurseRecords.Count(x => x.Content.Length > 40);
                    int fillingEmptyLine = fixedLinesNurse - (result.NurseRecords.Count + doubleLinesCount);

                    for (int i = 0; i < fillingEmptyLine; i++)
                    {
                        result.NurseRecords.Add(new NurseRecordData(userResolver));
                    }
                }
                if (!int.TryParse(config["Reports:Hemosheet:FixedLines:Med"], out int fixedLinesMed))
                {
                    fixedLinesMed = 0;
                }
                if (result.MedicineRecords.Count < fixedLinesMed)
                {
                    int fillingEmptyLine = fixedLinesNurse - result.MedicineRecords.Count;

                    for (int i = 0; i < fillingEmptyLine; i++)
                    {
                        result.MedicineRecords.Add(new MedicineRecordData(userResolver));
                    }
                }
                if (!int.TryParse(config["Reports:Hemosheet:FixedLines:Doctor"], out int fixedLinesDoctor))
                {
                    fixedLinesDoctor = 0;
                }
                if (result.DoctorRecords.Count < fixedLinesDoctor)
                {
                    int doubleLinesCount = result.DoctorRecords.Count(x => x.Content.Length > 40);
                    int fillingEmptyLine = fixedLinesDoctor - (result.DoctorRecords.Count + doubleLinesCount);

                    for (int i = 0; i < fillingEmptyLine; i++)
                    {
                        result.DoctorRecords.Add(new DoctorRecordData(userResolver));
                    }
                }

                // progress note fixed
                if (!int.TryParse(config["Reports:Hemosheet:FixedLines:ProgressNote"], out int fixedLinesProgressNote))
                {
                    fixedLinesProgressNote = 6;
                }
                int linesCount = result.ProgressNotes.Count > fixedLinesProgressNote ? result.ProgressNotes.Count : result.ProgressNotes.Sum(x => {
                    var a = x.A.Split('\n').Sum(line => (int)Math.Ceiling(line.Length / 35f));
                    var i = x.I.Split('\n').Sum(line => (int)Math.Ceiling(line.Length / 35f));
                    var e = x.E.Split('\n').Sum(line => (int)Math.Ceiling(line.Length / 35f));
                    return Math.Max(1, Math.Max(Math.Max(a, i), e));
                });

                if (linesCount < fixedLinesProgressNote){
                    int fillingEmptyLine = fixedLinesProgressNote - linesCount;
                    for (int i = 0; i < fillingEmptyLine; i++)
                    {
                        result.ProgressNotes.Add(new ProgressNoteData());
                    }
                }

                // ========== Nurses in shift =====================
                bool.TryParse(config["Reports:Hemosheet:NurseInShift"], out bool nurseInShiftEnabled);
                if (nurseInShiftEnabled)
                {
                    var nurses = hemosheet.GetNurseInShift(shiftUnit, userInfoService, patient, tz);
                    result.NursesInShift = nurses.Aggregate("", (x, y) => x + ", " + userResolver.GetName(y));
                    if (!string.IsNullOrEmpty(result.NursesInShift))
                    {
                        result.NursesInShift = result.NursesInShift.Remove(0, 2);
                    }
                }
                else
                {
                    result.NursesInShift = null;
                }

                // =========== Extra Mapping & Callback ===============

                await documentPlugins.ExecutePlugins(async handler => await handler.OnHemosheetMapping(hemosheet, result.Extras),
                    ex => logger.LogError(ex, "[PLUGIN] error at hemosheet mapping."));

                return result;
            }
            catch (Exception e)
            {
                logger.LogError($"Report preparing failed: {e.Message} || {e}");
                throw;
            }
        }
    }
}
