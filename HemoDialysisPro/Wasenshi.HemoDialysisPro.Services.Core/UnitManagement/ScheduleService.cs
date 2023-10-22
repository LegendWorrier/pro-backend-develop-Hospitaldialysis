using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Models.Enums;
using Wasenshi.HemoDialysisPro.Models.Settings;
using Wasenshi.HemoDialysisPro.Repositories.Interfaces;
using Wasenshi.HemoDialysisPro.Repositories.Repository;
using Wasenshi.HemoDialysisPro.Services.Interfaces;
using Wasenshi.HemoDialysisPro.Services.UnitManagement;
using Wasenshi.HemoDialysisPro.Models.ConfigExtesions;
using Wasenshi.HemoDialysisPro.Models.TimezoneUtil;
using Wasenshi.HemoDialysisPro.Utils;
using System.Linq.Expressions;

namespace Wasenshi.HemoDialysisPro.Services
{
    public class ScheduleService : UnitManagementServiceBase, IScheduleService
    {
        private readonly IConfiguration config;
        private readonly IMapper mapper;
        private readonly ISectionSlotPatientRepository slotPatientRepo;
        private readonly IScheduleProcessor processor;
        private readonly IWritableOptions<UnitSettings> settings;
        private readonly ILogger<ScheduleService> logger;

        private TimeZoneInfo tz;

        public ScheduleService(
            IConfiguration config,
            IMapper mapper,
            IScheduleUnitOfWork scheduleUOW,
            IShiftUnitOfWork shiftUOW,
            ISectionSlotPatientRepository slotPatientRepo,
            IScheduleProcessor processor,
            IMasterDataUOW masterdata,
            IWritableOptions<UnitSettings> settings,
            ILogger<ScheduleService> logger) : base(scheduleUOW, shiftUOW, masterdata)
        {
            this.config = config;
            this.mapper = mapper;
            this.slotPatientRepo = slotPatientRepo;
            this.processor = processor;
            this.settings = settings;
            this.logger = logger;

            tz = TimezoneUtils.GetTimeZone(config.GetValue<string>("TIMEZONE"));
        }

        public ScheduleSection GetSection(int sectionId)
        {
            return scheduleUOW.Section.Get(sectionId);
        }

        public IEnumerable<ScheduleSection> GetSections(int unitId)
        {
            var result = scheduleUOW.Section.Find(x => x.UnitId == unitId, false).OrderBy(x => x.StartTime).ToList();
            return result;
        }

        public int CreateOrUpdateSections(int unitId, IEnumerable<ScheduleSection> sections, IEnumerable<ScheduleSection> deletes)
        {
            _CreateOrUpdateSections(unitId, sections, deletes);

            return scheduleUOW.Complete();
        }

        private void _CreateOrUpdateSections(int unitId, IEnumerable<ScheduleSection> sections, IEnumerable<ScheduleSection> deletes)
        {
            sections = sections.OrderBy(x => x.StartTime);
            ScheduleSection previous = null;
            foreach (var item in sections)
            {
                item.UnitId = unitId;
                if (previous == null)
                {
                    previous = item;
                    continue;
                }
                // Safe-guard : section minimum duration is 4 hours by rule
                if ((item.StartTime - previous.StartTime).TotalMinutes < 240)
                {
                    throw new AppException("OVERLAP", "Cannot have overlapped sections. (minimum is 4 hours per section)");
                }
                previous = item;
            }

            scheduleUOW.Section.BulkDelete(deletes);
            scheduleUOW.Section.BulkInsertOrUpdate(sections);

            // ------------------ Save Meta Data -----------------------
            // if there is changes on the schedule sections, we need to save current version as a meta
            var tzNow = DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(DateTime.UtcNow, tz));
            var currentMeta = scheduleUOW.ShiftMeta.Find(x => x.ScheduleMeta.UnitId == unitId
                && x.Month.Year == tzNow.Year
                && x.Month.Month == tzNow.Month)
                .FirstOrDefault();
            if (currentMeta == null)
            {
                // if no shift meta for current month yet, create one.
                currentMeta = new ShiftMeta
                {
                    IsSystemUpdate = true,
                    Month = tzNow.AddDays(-tzNow.Day + 1), // save first day of this month
                    ScheduleMeta = CreateNewScheduleMeta(unitId, sections)
                };

                scheduleUOW.ShiftMeta.Insert(currentMeta);
            }
            else
            {
                // check if there are more than one month that is re-using current latest meta or not
                bool createNew = scheduleUOW.ShiftMeta.GetAll(false).Count(x => x.ScheduleMetaId == currentMeta.ScheduleMetaId) > 1;
                // if yes, save as new meta version
                if (createNew)
                {
                    var scheduleMeta = CreateNewScheduleMeta(unitId, sections);
                    currentMeta.ScheduleMetaId = scheduleMeta.Id;
                    currentMeta.ScheduleMeta = scheduleMeta;
                    currentMeta.IsSystemUpdate = true;
                    scheduleUOW.ShiftMeta.Update(currentMeta);
                }
                else // otherwise, override current latest's meta, as long as it is still the same month (same shift meta)
                {
                    var scheduleMeta = currentMeta.ScheduleMeta;
                    scheduleMeta.IsSystemUpdate = true;
                    UpdateScheduleMeta(scheduleMeta, sections);
                    scheduleUOW.ScheduleMeta.Update(scheduleMeta);
                }
            }
            // --------------------- End of Save Meta Data -----------------------
        }

        // ===================== Schedule Update (TempSection) =================================
        public IEnumerable<TempSection> GetTempSections(int unitId)
        {
            var tmpSections = scheduleUOW.TempSection.GetAll().Where(x => x.UnitId == unitId);
            return tmpSections.ToList();
        }

        public bool CreateOrUpdateTempSections(int unitId, IEnumerable<TempSection> tempSections)
        {
            tempSections = tempSections.OrderBy(x => x.StartTime);
            TempSection previous = null;
            foreach (var item in tempSections)
            {
                item.UnitId = unitId;
                if (item.Delete)
                {
                    continue;
                }
                if (previous == null)
                {
                    previous = item;
                    continue;
                }
                // Safe-guard : section minimum duration is 4 hours by rule
                if ((item.StartTime.Value - previous.StartTime.Value).TotalMinutes < 240)
                {
                    throw new AppException("OVERLAP", "Cannot have overlapped sections. (minimum is 4 hours per section)");
                }
                previous = item;
            }

            foreach (var item in tempSections)
            {
                bool existed = scheduleUOW.TempSection.Find(x => x.Id == item.Id).Any();
                if (existed) scheduleUOW.TempSection.Update(item);
                else scheduleUOW.TempSection.Insert(item);
            }

            return scheduleUOW.Complete() > 0;
        }

        public void ClearTempSections(int unitId)
        {
            var tempSections = GetTempSections(unitId);
            foreach (var item in tempSections)
            {
                scheduleUOW.TempSection.Delete(item);
            }

            scheduleUOW.Complete();
        }

        public void ApplyTempSections(int unitId)
        {
            var tempSections = GetTempSections(unitId);
            List<ScheduleSection> sections = new List<ScheduleSection>();
            List<ScheduleSection> deletes = new List<ScheduleSection>();
            foreach (var tempSection in tempSections)
            {
                if (tempSection.Delete)
                {
                    deletes.Add(mapper.Map<ScheduleSection>(tempSection));
                }
                else
                {
                    sections.Add(mapper.Map<ScheduleSection>(tempSection));
                }
            }
            _CreateOrUpdateSections(unitId, sections, deletes);
            ClearTempSections(unitId);
        }

        // ===================== End of Schedule Update (TempSection) =================================

        public int SlotPatientSchedule(int sectionId, SectionSlots slot, string patientId)
        {
            var unitId = scheduleUOW.Section.Get(sectionId).UnitId;
            CheckMaxPerSlot(unitId, sectionId, slot);

            var existingSlot = slotPatientRepo.Find(x => x.Slot == slot && x.PatientId == patientId, false).FirstOrDefault();
            if (existingSlot != null) // same slot day, same patient
            {
                throw new AppException("DUP_PATIENT", "Cannot slot the same patient in the same day again.");
            }
            var existingSchedules = scheduleUOW.Schedule.Find(x => x.PatientId == patientId, false).ToList();
            var tz = TimezoneUtils.GetTimeZone(config.GetValue<string>("TIMEZONE"));
            if (existingSchedules.Any(x => GetSectionSlotByDatetime(x.Date, tz) == slot))
            {
                throw new AppException("OCCUPIED_SLOT", "Too many patient's schedule existed on the slot day.");
            }

            var patientSlot = new SectionSlotPatient { PatientId = patientId, SectionId = sectionId, Slot = slot };
            slotPatientRepo.Insert(patientSlot);

            return slotPatientRepo.Complete();
        }

        public bool SwapSlot(SectionSlotPatient first, SectionSlotPatient second)
        {
            // if same slot patient
            if (first.PatientId == second.PatientId && first.SectionId == second.SectionId && first.Slot == second.Slot)
            {
                return false;
            }

            slotPatientRepo.Delete(first);
            var newFirst = new SectionSlotPatient { PatientId = first.PatientId, SectionId = second.SectionId, Slot = second.Slot };
            slotPatientRepo.Insert(newFirst);

            if (!string.IsNullOrWhiteSpace(second.PatientId)) // if swap two patient, no need to check max per slot
            {
                slotPatientRepo.Delete(second);
                var newSecond = new SectionSlotPatient { PatientId = second.PatientId, SectionId = first.SectionId, Slot = first.Slot };
                slotPatientRepo.Insert(newSecond);
            }
            else // in case moving slot, check max per slot also
            {
                var unitId = scheduleUOW.Section.Get(second.SectionId).UnitId;
                CheckMaxPerSlot(unitId, second.SectionId, second.Slot);
            }

            return slotPatientRepo.Complete() > 1;
        }

        public bool DeletePatientSlot(string patientId, int sectionId, SectionSlots slot)
        {
            var slotPatient = slotPatientRepo.Find(x => x.SectionId == sectionId && x.PatientId == patientId && x.Slot == slot).FirstOrDefault();
            slotPatientRepo.Delete(slotPatient);

            return slotPatientRepo.Complete() > 0;
        }

        public ScheduleResult GetSchedule(int unitId)
        {
            var patientSlots = slotPatientRepo.GetAll().Where(x => x.Section.UnitId == unitId).ToList();
            var sections = scheduleUOW.Section.Find(x => x.UnitId == unitId, false).OrderBy(x => x.StartTime);

            var result = processor.ProcessSlotData(sections, patientSlots);

            return result;
        }

        public IEnumerable<Schedule> Reschedule(IEnumerable<Schedule> schedules)
        {
            logger.LogDebug("reschedule timezone: " + tz.DisplayName + " (offset: " + tz.BaseUtcOffset + ")");
            List<SectionSlotPatient> slots = new List<SectionSlotPatient>();
            int i = 0;
            foreach (var item in schedules)
            {
                logger.LogDebug($"process schedule {++i}...");
                slots.Add(_ScheduleNew(item));
            }
            Dictionary<int, int> unitIdMap = new Dictionary<int, int>();
            int getUnitId(SectionSlotPatient original)
            {
                if (!unitIdMap.ContainsKey(original.SectionId))
                {
                    unitIdMap[original.SectionId] = scheduleUOW.Section.Get(original.SectionId).UnitId;
                }
                return unitIdMap[original.SectionId];
            }
            Dictionary<int, IEnumerable<ScheduleSection>> sectionMap = new Dictionary<int, IEnumerable<ScheduleSection>>();
            IEnumerable<ScheduleSection> getSectionsByUnitId(int unitId)
            {
                if (!sectionMap.ContainsKey(unitId))
                {
                    sectionMap[unitId] = scheduleUOW.Section.GetAll(false).Where(x => x.UnitId == unitId).ToList();
                }
                return sectionMap[unitId];
            }
            i = 0;
            foreach (var item in schedules)
            {
                var original = slots[i++];
                var unitId = item.OverrideUnitId ?? getUnitId(original);
                var max = GetMaxPatientPerSlot(unitId);
                var counts = CountForDate(unitId, item.Date);

                var targetDate = TimeZoneInfo.ConvertTime(new DateTimeOffset(item.Date, TimeSpan.Zero), tz);
                var lowerLimit = targetDate.AddTicks(-targetDate.TimeOfDay.Ticks);
                var upperLimit = lowerLimit.AddDays(1);

                var targetDay = GetSectionSlotByDatetime(item.Date, tz);
                var targetSection = GetSectionByDatetime(item.Date, tz, getSectionsByUnitId(unitId));

                var extraCount = schedules.Count(x => x.SectionId == targetSection.Id && x.Slot == targetDay
                    && x.OriginalDate.HasValue && x.OriginalDate.Value >= lowerLimit && x.OriginalDate.Value < upperLimit);
                if (counts.slotsCount - counts.otherScheduleCount - extraCount >= max)
                {
                    throw new AppException("MAX_NUMBER", "Max number per slot is reached.");
                }

                if (counts.slotsCount - counts.otherScheduleCount - extraCount + counts.scheduleCount >= max)
                {
                    throw new AppException("OVER_MAX", "Slot has too many occupied schedule(s).");
                }
            }

            scheduleUOW.Complete();

            return schedules;
        }

        private SectionSlotPatient _ScheduleNew(Schedule schedule)
        {
            var original = _VerifyAndFindOriginalSlot(schedule);

            scheduleUOW.Schedule.Insert(schedule);

            return original;
        }

        public IEnumerable<Schedule> GetReschedules(int unitId, Expression<Func<Schedule, bool>> whereCondition = null)
        {
            var reschedules = scheduleUOW.Schedule.GetAll()
                .Where(x => x.OverrideUnitId == unitId ||
                            x.Section.UnitId == unitId ||
                            x.Patient.UnitId == unitId);
            if (whereCondition != null)
            {
                reschedules = reschedules.Where(whereCondition);
            }

            return reschedules;
        }

        public IEnumerable<Schedule> GetActiveSchedules(int unitId, Expression<Func<Schedule, bool>> whereCondition = null)
        {
            return GetReschedules(unitId, whereCondition)
                .Where(x => x.Date > DateTime.UtcNow)
                .GroupBy(x => x.PatientId).Select(x => x.OrderBy(x => x.Date).First());
        }

        public IEnumerable<Schedule> GetActiveSchedulesForToday(IEnumerable<int> unitList, Expression<Func<Schedule, bool>> whereCondition = null)
        {
            var tzNow = TimeZoneInfo.ConvertTime(new DateTimeOffset(DateTime.UtcNow, TimeSpan.Zero), tz);
            var lowerLimit = tzNow.ToUtcDate();
            var upperLimit = lowerLimit.AddDays(1);

            var activeSchedule = scheduleUOW.Schedule.GetAll().Where(x => x.Date >= lowerLimit && x.Date < upperLimit);
            if (unitList.Any())
            {
                activeSchedule = activeSchedule.Where(x => unitList.Contains(x.OverrideUnitId.Value) || unitList.Contains(x.Section.UnitId) || unitList.Contains(x.Patient.UnitId));
            }
            if (whereCondition != null)
            {
                activeSchedule = activeSchedule.Where(whereCondition);
            }

            return activeSchedule.AsEnumerable().GroupBy(x => x.PatientId).Select(x => x.OrderBy(x => x.Date).First());
        }

        public IEnumerable<SectionSlotPatient> GetActiveSlotForToday(IEnumerable<int> unitList, Expression<Func<SectionSlotPatient, bool>> whereCondition = null)
        {
            var tzNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);
            SectionSlots todaySlot = tzNow.DayOfWeek == DayOfWeek.Sunday ? SectionSlots.Sun : (SectionSlots)tzNow.DayOfWeek - 1;
            var patientSlots = slotPatientRepo.GetAll().Where(x => x.Slot == todaySlot);
            if (unitList.Any())
            {
                patientSlots = patientSlots.Where(x => unitList.Any(u => u == x.Section.UnitId));
            }
            if (whereCondition != null)
            {
                patientSlots = patientSlots.Where(whereCondition);
            }

            return patientSlots;
        }

        public ScheduleCheck PatientCheckForToday(string patientId)
        {
            var tzNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz).AsUtcDate();
            var lowerLimit = tzNow.AddTicks(-tzNow.TimeOfDay.Ticks);
            var upperLimit = lowerLimit.AddDays(1);

            var slots = slotPatientRepo.GetAll(false).Where(x => x.PatientId == patientId).ToList();

            var hasActiveSchedule = scheduleUOW.Schedule.GetAll(false).Where(x => x.Date >= lowerLimit && x.Date < upperLimit && x.PatientId == patientId).Any();

            SectionSlots todaySlot = tzNow.DayOfWeek == DayOfWeek.Sunday ? SectionSlots.Sun : (SectionSlots)tzNow.DayOfWeek - 1;
            var hasActiveSlotToday = slots.Any(x => x.Slot == todaySlot);

            var closet = slots.Select(x => new { Slot = x, Distance = Math.Abs(x.Slot - todaySlot) }).OrderBy(x => x.Distance)
                .FirstOrDefault()?.Slot;

            return new ScheduleCheck
            {
                HasActiveToday = hasActiveSchedule || hasActiveSlotToday,
                ClosetSlot = closet
            };
        }

        private int GetMaxPatientPerSlot(int unitId)
        {
            int max = settings.GetOrDefault(unitId.ToString()).MaxPatientPerSlot.GetValueOrDefault();

            return max;
        }

        private void CheckMaxPerSlot(int unitId, int sectionId, SectionSlots slot)
        {
            var max = GetMaxPatientPerSlot(unitId);
            var count = slotPatientRepo.GetAll(false).Count(x => x.SectionId == sectionId && x.Slot == slot);
            if (count >= max)
            {
                throw new AppException("MAX_NUMBER", "Max number per slot is reached.");
            }

            var sections = scheduleUOW.Section.Find(x => x.UnitId == unitId).ToList();
            var tz = TimezoneUtils.GetTimeZone(config.GetValue<string>("TIMEZONE"));
            var today = TimeZoneInfo.ConvertTime(new DateTimeOffset(DateTime.UtcNow, TimeSpan.Zero), tz);
            var lowerLimit = today.AddTicks(-today.TimeOfDay.Ticks);
            var schedules = GetReschedules(unitId).Where(x => x.Date >= lowerLimit && (x.OverrideUnitId == null || x.OverrideUnitId == unitId)).ToList();
            // filter only on target week day and section (time), and group into each dates (can be on multiple dates)
            var group = schedules.Where(x =>
                (GetSectionByDatetime(x.Date, tz, sections)?.Id ?? 0) == sectionId && GetSectionSlotByDatetime(x.Date, tz) == slot)
                .GroupBy(x => x.Date.Day);
            if (group.Any(x => x.Count() + count >= max))
            {
                throw new AppException("OVER_MAX", "Slot has too many occupied schedule(s).");
            }
        }

        private (int slotsCount, int otherScheduleCount, int scheduleCount) CountForDate(int unitId, DateTime date)
        {
            var targetDate = TimeZoneInfo.ConvertTime(new DateTimeOffset(date, TimeSpan.Zero), tz);
            var lowerLimit = targetDate.AddTicks(-targetDate.TimeOfDay.Ticks).UtcDateTime;
            var upperLimit = lowerLimit.AddDays(1);

            var sections = scheduleUOW.Section.Find(x => x.UnitId == unitId).ToList();

            var sectionId = GetSectionByDatetime(date, tz, sections).Id;
            var slot = GetSectionSlotByDatetime(date, tz);

            var slotsCount = slotPatientRepo.GetAll(false).Count(x => x.SectionId == sectionId && x.Slot == slot);
            var otherScheduleCount = scheduleUOW.Schedule.GetAll(false).Count(x => x.SectionId == sectionId && x.Slot == slot
                && x.OriginalDate.HasValue && x.OriginalDate.Value >= lowerLimit && x.OriginalDate.Value < upperLimit);

            var schedules = GetReschedules(unitId).Where(x => x.OverrideUnitId == null && x.Date >= lowerLimit && x.Date < upperLimit).ToList(); // only target date
            int scheduleCount = schedules.Count(x => (GetSectionByDatetime(x.Date, tz, sections)?.Id ?? 0) == sectionId); // only target section (time)

            return (slotsCount, otherScheduleCount, scheduleCount);
        }

        private SectionSlots GetSectionSlotByDatetime(DateTime date, TimeZoneInfo tz)
        {
            var tzDate = TimeZoneInfo.ConvertTime(new DateTimeOffset(date, TimeSpan.Zero), tz);
            SectionSlots slot = tzDate.DayOfWeek == 0 ?
                    SectionSlots.Sun :
                    (SectionSlots)(tzDate.DayOfWeek - 1);
            return slot;
        }

        private ScheduleSection GetSectionByDatetime(DateTime date, TimeZoneInfo tz, IEnumerable<ScheduleSection> sections, bool throwIfNull = true)
        {
            var tzDate = TimeZoneInfo.ConvertTimeFromUtc(date, tz);
            logger.LogDebug("tzDate: " + tzDate);
            return GetSectionByDatetime(tzDate, sections, throwIfNull);
        }

        private ScheduleSection GetSectionByDatetime(DateTimeOffset date, IEnumerable<ScheduleSection> sections, bool throwIfNull = true)
        {
            var startTime = TimeOnly.FromTimeSpan(TimeSpan.FromMinutes(date.Hour * 60 + date.Minute));
            logger.LogDebug("startTime: " + startTime);
            var section = sections.Where(x => startTime.IsBetween(x.StartTime, x.StartTime.AddHours(4))).FirstOrDefault();

            if (section == null && throwIfNull)
            {
                throw new AppException("SECTION_NULL", "Cannot find section for target time.");
            }

            return section;
        }

        public Schedule GetPatientScheduleById(Guid id)
        {
            return scheduleUOW.Schedule.Get(id);
        }

        public bool DeleteSchedule(Guid scheduleId)
        {
            scheduleUOW.Schedule.Delete(new Schedule { Id = scheduleId });

            return scheduleUOW.Complete() > 0;
        }

        public bool IsCrossScheduleExisted(string patientId)
        {
            return scheduleUOW.Schedule.Find(x =>
                x.PatientId == patientId &&
                x.OverrideUnitId != null &&
                x.Date > DateTime.UtcNow)
                .Any();
        }

        public bool IsScheduleEmpty(int unitId)
        {
            bool hasSchedule = scheduleUOW.Schedule.GetAll().Any(x =>
                            (x.OverrideUnitId == unitId ||
                            x.Section.UnitId == unitId ||
                            x.Patient.UnitId == unitId) &&
                            x.Date > DateTime.UtcNow);
            bool hasSlot = slotPatientRepo.GetAll().Any(x => x.Patient.UnitId == unitId || x.Section.UnitId == unitId);

            return !hasSchedule && !hasSlot;
        }

        public void VerifySchedule(Schedule schedule)
        {
            _VerifyAndFindOriginalSlot(schedule);
        }

        private SectionSlotPatient _VerifyAndFindOriginalSlot(Schedule schedule)
        {
            if (schedule.Date < DateTime.UtcNow)
            {
                throw new AppException("PAST", "Cannot reschedule the past.");
            }
            if (schedule.OriginalDate.HasValue)
            {
                logger.LogDebug("has original date");
                Console.WriteLine("has original date");
                if (schedule.OriginalDate.Value < DateTime.UtcNow)
                {
                    throw new AppException("HISTORY", "Cannot reschedule the past.");
                }

                // auto find section and slot
                if (schedule.SectionId == 0)
                {
                    logger.LogDebug("section id is zero, meaning this should be auto find section...");
                    Console.WriteLine("section id is zero, meaning this should be auto find section...");
                    // controller needs to handle this patient unitId
                    var sections = GetSections(schedule.Patient.UnitId);
                    var section = GetSectionByDatetime(schedule.OriginalDate.Value, tz, sections);
                    schedule.SectionId = section?.Id ?? 0;
                    schedule.Slot = GetSectionSlotByDatetime(schedule.OriginalDate.Value, tz);
                    schedule.Patient = null;
                }
                else
                {
                    logger.LogDebug("check original date with sectionId && slot...");
                    Console.WriteLine("check original date with sectionId && slot...");
                    SectionSlots slot = GetSectionSlotByDatetime(schedule.OriginalDate.Value, tz);
                    var section = GetSectionByDatetime(schedule.OriginalDate.Value, tz, new[] { GetSection(schedule.SectionId) }, false);
                    if (slot != schedule.Slot || section == null)
                    {
                        throw new AppException("INVALID", "Invalid data. (Original date mismatched with original slot)");
                    }
                    logger.LogDebug("passed");
                }
            }
            if (scheduleUOW.Schedule.Find(x => x.PatientId == schedule.PatientId && x.Date == schedule.Date, false).Any())
            {
                throw new AppException("DUP_SCHEDULE", "Target date/time schedule already existed.");
            }

            var original = slotPatientRepo.Find(x =>
            x.PatientId == schedule.PatientId &&
            x.SectionId == schedule.SectionId &&
            x.Slot == schedule.Slot, false)
                .FirstOrDefault();
            if (original == null)
            {
                throw new AppException("INVALID", "Invalid slot. (doesn't exist)");
            }

            return original;
        }
    }
}
