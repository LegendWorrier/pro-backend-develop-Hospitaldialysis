using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ServiceStack.Messaging;
using ServiceStack.Redis;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Models.Settings;
using Wasenshi.HemoDialysisPro.Repositories.Interfaces;
using Wasenshi.HemoDialysisPro.Repositories.Interfaces.Base;
using Wasenshi.HemoDialysisPro.Services.BusinessLogic;
using Wasenshi.HemoDialysisPro.Services.Interfaces;
using Wasenshi.HemoDialysisPro.Share;
using Wasenshi.HemoDialysisPro.Models.ConfigExtesions;
using Wasenshi.HemoDialysisPro.Models.Enums;
using Wasenshi.HemoDialysisPro.Models.TimezoneUtil;
using Wasenshi.HemoDialysisPro.Utils;
using Wasenshi.HemoDialysisPro.Services.Core.Interfaces;
using Wasenshi.HemoDialysisPro.PluginBase;
using Wasenshi.HemoDialysisPro.PluginIntegrate;

namespace Wasenshi.HemoDialysisPro.Services
{
    public class HemoService : IHemoService
    {
        private readonly IHemoUnitOfWork hemoUOW;
        private readonly IShiftUnitOfWork shiftUnit;
        private readonly IAssessmentRepository assessmentRepo;
        private readonly IAssessmentItemRepository assessmentItemRepo;
        private readonly IPatientRepository patientRepo;
        private readonly IUserInfoService userInfoService;
        private readonly IDialysisRecordRepository dialysisRecordRepo;
        private readonly IRepository<HemoNote, Guid> hemoNoteRepo;
        private readonly ISystemBoundLabProcessor systemBoundLabProcessor;
        private readonly IRedisClient redis;
        private readonly IMessageQueueClient message;
        private readonly IEnumerable<IDocumentHandler> docPlugins;
        private readonly IMapper mapper;
        private readonly IConfiguration config;
        private readonly IWritableOptions<GlobalSetting> setting;
        private readonly IWritableOptions<UnitSettings> unitSettings;
        private readonly ILogger<HemoService> logger;
        private readonly TimeZoneInfo tz;

        public HemoService(
            IHemoUnitOfWork hemoUOW,
            IShiftUnitOfWork shiftUnit,
            IAssessmentRepository assessmentRepo,
            IAssessmentItemRepository assessmentItemRepo,
            IPatientRepository patientRepo,
            IUserInfoService userInfoService,
            IDialysisRecordRepository dialysisRecordRepo,
            IRepository<HemoNote, Guid> hemoNoteRepo,
            ISystemBoundLabProcessor systemBoundLabProcessor,
            IRedisClient redis,
            IMessageQueueClient message,
            IEnumerable<IDocumentHandler> docPlugins,
            IMapper mapper,
            IConfiguration config,
            IWritableOptions<GlobalSetting> setting,
            IWritableOptions<UnitSettings> unitSettings,
            ILogger<HemoService> logger)
        {
            this.hemoUOW = hemoUOW;
            this.shiftUnit = shiftUnit;
            this.assessmentRepo = assessmentRepo;
            this.assessmentItemRepo = assessmentItemRepo;
            this.patientRepo = patientRepo;
            this.userInfoService = userInfoService;
            this.dialysisRecordRepo = dialysisRecordRepo;
            this.hemoNoteRepo = hemoNoteRepo;
            this.systemBoundLabProcessor = systemBoundLabProcessor;
            this.redis = redis;
            this.message = message;
            this.docPlugins = docPlugins;
            this.mapper = mapper;
            this.config = config;
            this.setting = setting;
            this.unitSettings = unitSettings;
            this.logger = logger;
            tz = TimezoneUtils.GetTimeZone(config.GetValue<string>("TIMEZONE"));
        }

        public Page<HemoRecordResult> GetAllHemodialysisRecords(int page = 1, int limit = 25,
            Action<IOrderer<HemoRecordResult>> orderBy = null,
            Expression<Func<HemoRecordResult, bool>> condition = null)
        {
            IQueryable<HemoRecordResult> allRecords = hemoUOW.HemoRecord.GetAllWithPatient();

            void Ordering(IOrderer<HemoRecordResult> orderer)
            {
                orderer.Default(x => x.Record.Created, true); // Default order by date with latest first
                orderBy?.Invoke(orderer); // Followed by custom ordering
            }

            var result = allRecords.GetPagination(limit, page - 1, Ordering, condition);

            return result;
        }

        public int CountAll(Expression<Func<HemoRecordResult, bool>> whereCondition = null)
        {
            IQueryable<HemoRecordResult> getAll = hemoUOW.HemoRecord.GetAllWithPatient(false);
            if (whereCondition != null)
            {
                getAll = getAll.Where(whereCondition);
            }
            return getAll.Count();
        }

        public Page<HemodialysisRecord> GetAllHemodialysisRecordsWithNote(int page = 1, int limit = 25,
           Action<IOrderer<HemodialysisRecord>> orderBy = null,
           Expression<Func<HemodialysisRecord, bool>> condition = null)
        {
            IQueryable<HemodialysisRecord> allRecords = hemoUOW.HemoRecord.GetAllWithNote();

            void Ordering(IOrderer<HemodialysisRecord> orderer)
            {
                orderer.Default(x => x.CompletedTime, true); // Default order by date with latest first
                orderBy?.Invoke(orderer); // Followed by custom ordering
            }

            var result = allRecords.GetPagination(limit, page - 1, Ordering, condition);

            return result;
        }

        public HemodialysisRecord CreateHemodialysisRecord(HemodialysisRecord hemoRecord, ScheduleSection shiftSection = null)
        {
            // safe guard : cannot create multiple active hemosheet
            if (GetHemodialysisRecordByPatientId(hemoRecord.PatientId) != null)
            {
                throw new InvalidOperationException("Cannot create new record, because there already is an active hemosheet.");
            }

            hemoRecord.Id = Guid.Empty;
            hemoUOW.HemoRecord.Insert(hemoRecord);

            DialysisPrescription prescription = GetLatestDialysisPrescriptionByPatientId(hemoRecord.PatientId);
            if (prescription != null)
            {
                // We need to put only id here, because if we put prescription into the model, the EF will also track the prescription
                hemoRecord.DialysisPrescriptionId = prescription.Id;
            }

            // insert current shift as default for newly created hemosheet
            if (shiftSection != null)
            {
                hemoRecord.ShiftSectionId = shiftSection.Id;
            }

            // init default assessment
            bool hasRe = config.GetValue<bool>("Reports:Hemosheet:HasReassessment");
            var items = new List<AssessmentItem>();

            var boolAssessments = assessmentRepo.GetAll().Where(x => !x.Multi && x.OptionType == OptionTypes.Checkbox);
#pragma warning disable S3267 // Loops should be simplified with "LINQ" expressions
            foreach (Assessment assessment in boolAssessments)
            {
                items.Add(new AssessmentItem
                {
                    HemosheetId = hemoRecord.Id,
                    AssessmentId = assessment.Id,
                    Checked = false, //Default
                    IsSystemUpdate = true
                });
                if (hasRe)
                {
                    // for reassessment
                    items.Add(new AssessmentItem
                    {
                        HemosheetId = hemoRecord.Id,
                        AssessmentId = assessment.Id,
                        Checked = false, //Default,
                        IsReassessment = true,
                        IsSystemUpdate = true
                    });
                }
            }
#pragma warning restore S3267 // Loops should be simplified with "LINQ" expressions
            var optionAssessments = assessmentRepo.GetAll().Where(x => x.OptionsList.Any());
            foreach (Assessment assessment in optionAssessments)
            {
                var defaultItems = assessment.OptionsList.Where(x => x.IsDefault).Select(x => x.Id).ToArray();
                if (defaultItems.Any())
                {
                    items.Add(new AssessmentItem
                    {
                        HemosheetId = hemoRecord.Id,
                        AssessmentId = assessment.Id,
                        Selected = defaultItems,
                        IsSystemUpdate = true
                    });
                }
            }
            assessmentItemRepo.BulkInsertOrUpdate(items);

            hemoUOW.Complete();

            // now that we have already commited and save change onto the database, put prescription back into the model to save database round-trip
            hemoRecord.DialysisPrescription = prescription;
            ServiceEvents.DispatchCreate(hemoRecord);
            docPlugins.ExecutePluginsOnBackgroundThread(async (doc) => await doc.OnHemosheetCreated(hemoRecord));

            // (KTV need post weight, so on creation, there no need to update KTV)

            return hemoRecord;
        }

        public HemodialysisRecord GetHemodialysisRecord(Guid recordId)
        {
            var result = hemoUOW.HemoRecord.Get(recordId);
            return result;
        }

        public bool UpdateDoctorConsent(Guid recordId, bool consent = true)
        {
            var hemoRecord = GetHemodialysisRecord(recordId);
            // update doctor consent
            hemoRecord.DoctorConsent = consent;
            var entity = hemoUOW.HemoRecord.Update(hemoRecord);

            var prescription = entity.Reference(x => x.DialysisPrescription);
            if (prescription.TargetEntry != null)
            {
                prescription.TargetEntry.State = EntityState.Unchanged;
            }

            return hemoUOW.Complete() > 0;
        }

        public bool ClaimHemosheet(Guid recordId, Guid userId)
        {
            var hemoRecord = GetHemodialysisRecord(recordId);
            if (hemoRecord == null)
            {
                return false;
            }

            if (hemoRecord.CreatedBy != Guid.Empty)
            {
                return false;
            }
            hemoRecord.CreatedBy = userId;
            var entity = hemoUOW.HemoRecord.Update(hemoRecord);

            var prescription = entity.Reference(x => x.DialysisPrescription);
            if (prescription.TargetEntry != null)
            {
                prescription.TargetEntry.State = EntityState.Unchanged;
            }

            return hemoUOW.Complete() > 0;
        }

        public DialysisPrescription CreatePrescription(DialysisPrescription prescription)
        {
            if (!patientRepo.GetAll(false).Any(x => x.Id == prescription.PatientId))
            {
                throw new AppException("NULL", "Cannot find the patient.");
            }

            hemoUOW.Prescription.Insert(prescription);

            hemoUOW.Complete();

            if (FeatureFlag.HasIntegrated() && hemoUOW.Prescription.GetAll(false).Count(x => x.PatientId == prescription.PatientId) > 1)
            {
                var patient = patientRepo.GetAll(false).First(x => x.Id == prescription.PatientId);
                var target = NotificationTarget.ForNurses(patient.UnitId);
                var noti = redis.AddNotification(
                    "PresNew_title",
                    $"PresNew_detail::{patient.Name}",
                    new[] { "page", "patient", patient.Id, "pres" },
                    target,
                    "pres-new"
                    );
                message.SendNotificationEvent(noti, target);
            }

            return prescription;
        }

        public bool DeletePrescription(Guid id)
        {
            var prescription = hemoUOW.Prescription.Get(id) ?? throw new KeyNotFoundException();
            if (prescription != null)
            {
                prescription.IsActive = false;
                hemoUOW.Prescription.Update(prescription);
            }

            int result;

            // update on-going hemosheet and lab calculation
            // Because if there is no total hour value from prescription, we cannot calculate KTV
            var hemoRecord = prescription.HemodialysisRecords.Where(x => x.CompletedTime == null).OrderByDescending(x => x.Created).FirstOrDefault();
            if (hemoRecord != null)
            {
                hemoRecord.DialysisPrescriptionId = null;
                hemoUOW.HemoRecord.Update(hemoRecord);
                result = hemoUOW.Complete();

                systemBoundLabProcessor.CleanBUNCalculation(hemoRecord);
                systemBoundLabProcessor.Commit();
            }
            else
            {
                result = hemoUOW.Complete();
            }

            if (result > 0 && FeatureFlag.HasIntegrated())
            {
                var patient = patientRepo.GetAll(false).First(x => x.Id == prescription.PatientId);
                var target = NotificationTarget.ForNurses(patient.UnitId);
                var noti = redis.AddNotification(
                    "PresDel_title",
                    $"PresDel_detail::{patient.Name}",
                    new[] { "page", "patient", patient.Id, "pres" },
                    target,
                    "pres-del"
                    );
                message.SendNotificationEvent(noti, target);
            }

            return result > 0;
        }

        public bool EditPrescription(DialysisPrescription prescription)
        {
            var old = hemoUOW.Prescription.Get(prescription.Id);

            // Safe guard, cannot edit history
            if (old.HemodialysisRecords?.Any(x => x.CompletedTime != null) == true)
            {
                throw new InvalidOperationException("Cannot edit history.");
            }
            // Safe guard, cannot edit patientId!
            if (old.PatientId != prescription.PatientId)
            {
                throw new InvalidOperationException("Cannot edit patient Id.");
            }

            hemoUOW.Prescription.Update(prescription);

            int result = hemoUOW.Complete();

            // update lab calculation
            var hemoRecord = old.HemodialysisRecords.OrderByDescending(x => x.Created).FirstOrDefault();
            if (hemoRecord != null)
            {
                systemBoundLabProcessor.ProcessBUN(hemoRecord);
                systemBoundLabProcessor.Commit();
            }

            if (result > 0 && FeatureFlag.HasIntegrated())
            {
                var patient = patientRepo.GetAll(false).FirstOrDefault(x => x.Id == prescription.PatientId) ?? throw new AppException("NULL", "Cannot find the associated patient.");
                var target = NotificationTarget.ForNurses(patient.UnitId);
                var noti = redis.AddNotification(
                    "PresEdit_title",
                    $"PresEdit_detail::{patient.Name}",
                    new[] { "page", "patient", patient.Id, "pres" },
                    target,
                    "pres-edit"
                    );
                message.SendNotificationEvent(noti, target);
            }

            return result > 0;
        }

        public bool EditHemodialysisRecord(HemodialysisRecord hemoRecord, bool markCompleted = false)
        {
            // Safe guard : invalid data
            if (hemoRecord.DialysisPrescription == null && hemoRecord.DialysisPrescriptionId != null)
            {
                if (!markCompleted && hemoRecord.CompletedTime != null)
                {
                    throw new InvalidOperationException("Cannot edit prescription of completed data.");
                }
                var checkExist = GetDialysisPrescription(hemoRecord.DialysisPrescriptionId.Value);
                if (checkExist?.IsActive != true)
                {
                    throw new InvalidOperationException("Invalid dialysis prescription. (deleted or not existed)");
                }

                if (setting.Value.Hemosheet.Rules.ChangePrescriptionSensitive)
                {
                    hemoRecord.DoctorConsent = false;
                }
            }

            if (markCompleted)
            {
                if (!hemoRecord.CompletedTime.HasValue)
                {
                    hemoRecord.CompletedTime = DateTime.UtcNow;
                }
                var patient = patientRepo.Get(hemoRecord.PatientId);
                bool autoNurseInShift = unitSettings.GetOrDefault(patient.UnitId.ToString()).AutoNurseInShift.Value;
                if (autoNurseInShift)
                {
                    // save nurses in shift
                    hemoRecord.NursesInShift = hemoRecord.GetNurseInShift(shiftUnit, userInfoService, patient, tz, true);
                }

                // Save doctor (lock), may need way to unlock this value in the future.
                hemoRecord.DoctorId = patientRepo.Get(hemoRecord.PatientId).DoctorId;
                hemoRecord.TreatmentNo = CountAll(x => x.Patient.Id == hemoRecord.PatientId) + (patient.DialysisInfo?.AccumulatedTreatmentTimes ?? 0);
                // use no for dialyzer
                if (hemoRecord.Dialyzer.UseNo < 1)
                {
                    hemoRecord.Dialyzer.UseNo = 1;
                }
            }

            hemoUOW.HemoRecord.SyncCollection(hemoRecord, x => x.PostVitalsign, new VitalSignComparer());
            hemoUOW.HemoRecord.SyncCollection(hemoRecord, x => x.PreVitalsign, new VitalSignComparer());
            var entity = hemoUOW.HemoRecord.Update(hemoRecord);

            var prescription = entity.Reference(x => x.DialysisPrescription);
            if (prescription.TargetEntry != null)
            {
                prescription.TargetEntry.State = EntityState.Unchanged;
            }

            int result = hemoUOW.Complete();

            if (result > 0)
            {
                var created = TimeZoneInfo.ConvertTime(new DateTimeOffset(hemoRecord.Created.Value, TimeSpan.Zero), tz);
                var upperLimit = created.AddDays(1).ToUtcDate();
                if (hemoRecord.CompletedTime == null ||
                    !hemoUOW.HemoRecord.GetAll(false).Any(x =>
                    x.Id != hemoRecord.Id &&
                    x.PatientId == hemoRecord.PatientId &&
                    x.Dehydration.PostTotalWeight > 0 &&
                    (
                        x.CompletedTime == null ||
                        (x.Created.Value > hemoRecord.Created.Value && x.Created.Value < upperLimit)
                    )
                    )) // safe-gaurd check that this hemo is the last of the day, because there may be more than one hemosheet on the same day
                {
                    // If this is the last hemosheet of the day, update lab calculation
                    if (hemoRecord.Dehydration.PostTotalWeight <= 0)
                    {
                        // If post weight is zero, KTV cannot be calculated. So delete it.
                        systemBoundLabProcessor.CleanBUNCalculation(hemoRecord);
                        systemBoundLabProcessor.Commit();
                    }
                    else
                    {
                        systemBoundLabProcessor.ProcessBUN(hemoRecord);
                        systemBoundLabProcessor.Commit();
                    }
                }

                if (markCompleted)
                {
                    ServiceEvents.DispatchComplete(hemoRecord);
                    docPlugins.ExecutePluginsOnBackgroundThread(async (doc) => await doc.OnHemosheetComplete(hemoRecord));
                }
            }

            return result > 0;
        }

        public bool CompleteHemodialysisRecord(Guid id, HemodialysisRecord update = null)
        {
            var record = hemoUOW.HemoRecord.Get(id);
            if (record == null)
            {
                return false;
            }
            if (record.CompletedTime != null)
            {
                logger.LogInformation("Attempted to complete an already completed hemosheet. [{RecordId}]", record.Id);
                return false;
            }
            if (record.DialysisPrescription == null && (update.DialysisPrescriptionId == null || update.DialysisPrescriptionId == Guid.Empty))
            {
                throw new InvalidOperationException("Dialysis Prescription cannot be null.");
            }

            HemodialysisRecord edited = record;

            if (update != null)
            {
                edited = mapper.Map(update, record);
                if (edited != null)
                {
                    edited.Id = id;
                    if (edited.DialysisPrescription?.Id != edited.DialysisPrescriptionId)
                    {
                        edited.DialysisPrescription = null;
                    }
                }
            }

            Debug.Assert(edited != null, nameof(edited) + " != null");

            // auto update cycle start/end times
            var records = dialysisRecordRepo.GetAll(false)
                .Where(x => x.HemodialysisId == edited.Id)
                .OrderBy(x => x.Timestamp)
                .ToList();
            if (records.Any())
            {
                var startTime = records[0].Timestamp;
                var endTime = records.Last().Timestamp;
                if (edited.CycleStartTime.HasValue && (edited.CycleStartTime.Value - startTime).Duration().TotalHours > 1)
                {
                    // trigger update nurses in shift if start time is changed dramatically.
                    edited.ShiftSectionId = 0;
                }
                edited.CycleStartTime = startTime;
                edited.CycleEndTime = endTime;
            }
            else
            {
                if (edited.CycleStartTime == null)
                    edited.CycleStartTime = edited.Created;
                if (edited.CycleEndTime == null)
                    edited.CycleEndTime = DateTime.UtcNow;
            }

            // auto execute any not-executed records
            var executeRecords = hemoUOW.ExecutionRecord.GetAll(false)
                .Where(x => x.HemodialysisId == edited.Id)
                .ToList();
            var now = DateTime.UtcNow;
            foreach (var executionRecord in executeRecords)
            {
                if (executionRecord.IsExecuted) continue;

                executionRecord.IsExecuted = true;
                executionRecord.Timestamp = now;
                hemoUOW.ExecutionRecord.Update(executionRecord);
            }

            return EditHemodialysisRecord(edited, true);
        }

        public bool ChangeCompleteTime(Guid hemoId, DateTimeOffset newTime)
        {
            var record = hemoUOW.HemoRecord.Get(hemoId);
            if (record == null)
            {
                return false;
            }
            if (record.CompletedTime == null)
            {
                throw new InvalidOperationException("The target hemosheet is still on going.");
            }

            record.CompletedTime = newTime.UtcDateTime;
            hemoUOW.HemoRecord.Update(record);
            hemoUOW.Complete();

            return true;
        }

        public bool DeleteHemosheet(Guid hemoId)
        {
            hemoUOW.HemoRecord.Delete(new HemodialysisRecord { Id = hemoId });

            return hemoUOW.Complete() > 0;
        }

        public IEnumerable<DialysisPrescription> GetDialysisPrescriptionsByPatientId(string patientId)
        {
            var result = hemoUOW.Prescription.GetAll()
                .Where(x => x.PatientId == patientId)
                .OrderByDescending(x => x.Created)
                .ToList();
            return result;
        }

        public Page<HemodialysisRecord> GetHemodialysisRecordsByPatientId(string patientId, int page = 1, int limit = 25, Expression<Func<HemodialysisRecord, bool>> whereCondition = null)
        {
            Expression<Func<HemodialysisRecord, bool>> where = x => x.PatientId == patientId;
            Expression<Func<HemodialysisRecord, bool>> extraCondition = whereCondition?.OrElse(x => x.CompletedTime == null);

            var result = hemoUOW.HemoRecord.GetAll()
                .GetPagination(limit, page - 1,
                    orderer => orderer.OrderBy(x => x.CompletedTime, true).OrderBy(x => x.Created),
                    where.AndAlso(extraCondition));
            return result;
        }

        public DialysisPrescription GetLatestDialysisPrescriptionByPatientId(string patientId)
        {
            var result = hemoUOW.Prescription.GetAll()
                .OrderByDescending(x => x.Created)
                .FirstOrDefault(LatestDialysisPrescription(patientId));
            return result;
        }

        public HemodialysisRecord GetHemodialysisRecordByPatientId(string patientId)
        {
            var result = hemoUOW.HemoRecord.GetAll()
                .Where(x => x.PatientId == patientId && x.CompletedTime == null)
                .OrderByDescending(x => x.Created)
                .FirstOrDefault();
            return result;
        }

        public DialysisPrescription GetDialysisPrescription(Guid prescriptionId)
        {
            var result = hemoUOW.Prescription.Get(prescriptionId);
            return result;
        }

        public bool CheckDialysisPrescriptionExists(string patientId)
        {
            return hemoUOW.Prescription.GetAll(false).Any(LatestDialysisPrescription(patientId));
        }

        public HemodialysisRecord GetPreviousHemosheet(HemodialysisRecord hemosheet)
        {
            if (hemosheet?.PatientId == null)
            {
                return null;
            }
            var result = hemoUOW.HemoRecord.GetAll()
                .Where(x => x.PatientId == hemosheet.PatientId && x.Created < hemosheet.Created)
                .OrderByDescending(x => x.Created)
                .FirstOrDefault();
            return result;
        }

        public bool UpdateNurseInShift(Guid hemoId, IEnumerable<Guid> nursesList)
        {
            bool nurseInShiftEnabled = config.GetValue("Reports:Hemosheet:NurseInShift", false);
            if (!nurseInShiftEnabled)
            {
                return false;
            }
            var record = hemoUOW.HemoRecord.Get(hemoId);
            if (record == null)
            {
                return false;
            }
            if (record.CompletedTime == null)
            {
                return false;
            }

            record.NursesInShift = nursesList.ToArray();

            hemoUOW.HemoRecord.Update(record);
            hemoUOW.Complete();

            return true;
        }

        public HemoNote UpdateHemoNote(HemoNote hemoNote)
        {
            hemoNoteRepo.Update(hemoNote);
            hemoNoteRepo.Complete();

            return hemoNote;
        }

        // =========================== Utils Functions ==============================

        private static Expression<Func<DialysisPrescription, bool>> LatestDialysisPrescription(string patientId)
        {
            return (x) =>
                x.PatientId == patientId && x.IsActive &&
                (!x.Temporary || (x.Temporary && !x.HemodialysisRecords.Any()));
        }
    }
}
