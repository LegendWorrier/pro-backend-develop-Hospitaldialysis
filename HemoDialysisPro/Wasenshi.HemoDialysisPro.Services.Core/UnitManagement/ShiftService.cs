using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Models.Settings;
using Wasenshi.HemoDialysisPro.Repositories.Interfaces;
using Wasenshi.HemoDialysisPro.Services.Core.Interfaces;
using Wasenshi.HemoDialysisPro.Services.Interfaces;
using Wasenshi.HemoDialysisPro.Services.UnitManagement;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Models.ConfigExtesions;
using Wasenshi.HemoDialysisPro.Models.TimezoneUtil;
using Wasenshi.HemoDialysisPro.Utils;

namespace Wasenshi.HemoDialysisPro.Services
{
    public class ShiftService : UnitManagementServiceBase, IShiftService
    {
        private readonly IConfiguration config;
        private readonly IShiftProcessor shiftProcessor;
        private readonly IUserInfoService userInfoService;
        private readonly IWritableOptions<UnitSettings> settings;
        private readonly ILogger<ScheduleService> logger;

        private TimeZoneInfo tz;

        public ShiftService(
            IConfiguration config,
            IScheduleUnitOfWork scheduleUOW,
            IShiftUnitOfWork shiftUOW,
            IMasterDataUOW masterdata,
            IShiftProcessor shiftProcessor,
            IUserInfoService userInfoService,
            IWritableOptions<UnitSettings> settings,
            ILogger<ScheduleService> logger) : base(scheduleUOW, shiftUOW, masterdata)
        {
            this.config = config;
            this.shiftProcessor = shiftProcessor;
            this.userInfoService = userInfoService;
            this.settings = settings;
            this.logger = logger;

            tz = TimezoneUtils.GetTimeZone(config.GetValue<string>("TIMEZONE"));
        }

        public bool AddOrUpdateIncharge(IEnumerable<ShiftIncharge> incharges)
        {
            foreach (var item in incharges)
            {
                shiftUOW.ShiftIncharge.AddOrUpdate(item);
            }

            return shiftUOW.Complete() > 0;
        }

        public bool ClearIncharge(DateOnly? month)
        {
            var tzNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);
            Expression<Func<ShiftIncharge, bool>> filter = month.HasValue ?
                ((ShiftIncharge x) => x.Date.Month == month.Value.Month && x.Date.Year == month.Value.Year) :
                ((ShiftIncharge x) => x.Date.Month < tzNow.Month);
            var records = shiftUOW.ShiftIncharge.Find(filter, false);
            foreach (var item in records)
            {
                shiftUOW.ShiftIncharge.Delete(item);
            }

            return shiftUOW.ShiftIncharge.Complete() > 0;
        }

        public bool IsIncharge(Guid userId, int unitId, ScheduleSection overrideSection = null)
        {
            var tzNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);
            var record = shiftUOW.ShiftIncharge.Find(x => x.UnitId == unitId
                && x.Date.Year == tzNow.Year && x.Date.Month == tzNow.Month && x.Date.Day == tzNow.Day, overrideSection == null).FirstOrDefault();
            return _IsInCharge(userId, record, new[] { overrideSection });
        }

        public bool hasIncharge(Guid userId, IEnumerable<ScheduleSection> scheduleSections)
        {
            var tzNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);
            var currentDate = DateOnly.FromDateTime(tzNow);
            var records = shiftUOW.ShiftIncharge
                .Find(x => x.Date == currentDate)
                .ToList();
            foreach (var item in records)
            {
                bool isIncharge = _IsInCharge(userId, item, scheduleSections);
                if (isIncharge)
                {
                    return true;
                }
            }

            return false;
        }

        private bool _IsInCharge(Guid userId, ShiftIncharge record, IEnumerable<ScheduleSection> currentSections = null)
        {
            if (record == null)
            {
                return false;
            }
            if (record.UserId == userId && record.Sections.Count() == 0)
            {
                return true;
            }

            var tzNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);
            ShiftInchargeSection incharge;
            if (currentSections != null)
            {
                incharge = record.Sections.FirstOrDefault(x => currentSections.Any(s => s.Id == x.SectionId));
            }
            else
            {
                var currentTime = TimeOnly.FromTimeSpan(tzNow.TimeOfDay);
                incharge = record.Sections.OrderByDescending(x => x.Section.StartTime).FirstOrDefault(x => x.Section.StartTime <= currentTime);
            }
            if (incharge?.UserId == userId || (incharge == null && record.UserId == userId))
            {
                return true;
            }

            return false;
        }

        public IEnumerable<ShiftSlot> CreateOrUpdateShift(DateOnly month, IEnumerable<ShiftSlot> slots, IEnumerable<UserShift> suspendList)
        {
            // Safe-Guard : Cannot edit history
            var allowHistorySave = config.GetValue<bool>("TESTING");
            if (!allowHistorySave && TimeZoneInfo.ConvertTime(new DateTimeOffset(DateTime.UtcNow, TimeSpan.Zero), tz).Month > month.Month)
            {
                throw new AppException("SHIFT_HISTORY", "Cannot edit history");
            }
            month = month.AddDays(-month.Day + 1); // to first day of the month (this already converted to equavalent tzMonth)
            Dictionary<int, ShiftMeta> metaDict = new Dictionary<int, ShiftMeta>();

            // -------------------- Update Shift Meta for all units ------------------------------
            var units = masterdata.GetMasterRepo<Unit, int>().GetAll().ToList();
            foreach (var unit in units)
            {
                var shiftMeta = shiftUOW.ShiftMeta.Find(x => x.ScheduleMeta.UnitId == unit.Id && x.Month.Year == month.Year && x.Month.Month == month.Month).FirstOrDefault();
                // first init this month shift meta
                if (shiftMeta == null)
                {
                    // use current latest schedule meta
                    var scheduleMeta = shiftUOW.ScheduleMeta.Find(x => x.UnitId == unit.Id).OrderByDescending(x => x.Created).FirstOrDefault();
                    if (scheduleMeta == null)
                    {
                        // if no meta found, create the current schedule version as first meta
                        var sections = scheduleUOW.Section.GetAll(false).Where(x => x.UnitId == unit.Id).ToList();
                        scheduleMeta = CreateNewScheduleMeta(unit, sections);
                    }
                    shiftMeta = new ShiftMeta
                    {
                        IsSystemUpdate = true,
                        Month = month
                    };
                    if (scheduleMeta.Id != 0)
                    {
                        shiftMeta.ScheduleMetaId = scheduleMeta.Id;
                    }
                    else
                    {
                        shiftMeta.ScheduleMeta = scheduleMeta;
                    }
                    scheduleUOW.ShiftMeta.Insert(shiftMeta);
                }
                metaDict.Add(unit.Id, shiftMeta);
            }

            // set meta for all shift slots
            foreach (var item in slots)
            {
                var targetUnitId = item.ShiftMeta?.ScheduleMeta?.UnitId;
                if (targetUnitId != null && targetUnitId.Value != 0)
                {
                    var meta = metaDict[targetUnitId.Value];
                    if (meta.Id != 0)
                    {
                        item.ShiftMetaId = meta.Id;
                        item.ShiftMeta = null;
                    }
                    else
                    {
                        item.ShiftMetaId = null;
                        item.ShiftMeta = meta;
                    }
                }
                else
                {
                    item.ShiftMetaId = null;
                    item.ShiftMeta = null;
                }
                item.Date = month.AddDays(item.Date.Day - 1); // ensure same month
                shiftUOW.ShiftSlot.Update(item);
            }

            _suspendUsers(suspendList, month);

            shiftUOW.Complete();
            return slots;
        }
        public IEnumerable<UserShift> SuspendUser(IEnumerable<UserShift> suspendList, DateOnly month)
        {
            _suspendUsers(suspendList, month);

            shiftUOW.Complete();
            return suspendList;
        }

        private void _suspendUsers(IEnumerable<UserShift> suspendList, DateOnly month)
        {
            if (suspendList == null)
            {
                return;
            }

            foreach (var item in suspendList)
            {
                item.Month = month;
                shiftUOW.UserShift.Update(item);
            }
        }

        public IEnumerable<DateOnly> GetHistoryList(IEnumerable<int> unitFilter = null)
        {
            var tzNow = TimeZoneInfo.ConvertTime(new DateTimeOffset(DateTime.UtcNow, TimeSpan.Zero), tz);
            var thisMonth = DateOnly.FromDateTime(tzNow.DateTime);
            var startOfThisMonth = thisMonth.AddDays(-thisMonth.Day + 1);
            Expression<Func<ShiftSlot, bool>> @where = unitFilter?.Any() ?? false ?
                ((ShiftSlot x) => x.ShiftMeta == null || unitFilter.Any(unitId => unitId == x.ShiftMeta.ScheduleMeta.UnitId))
                : _ => true;
            var list = shiftUOW.ShiftSlot.GetAll().Where(@where).Where(x => x.Date < startOfThisMonth)
                .GroupBy(x => new { Year = x.Date.Year, Month = x.Date.Month })
                .OrderByDescending(x => x.Key.Year)
                .ThenByDescending(x => x.Key.Month)
                .Select(x => x.Key)
                .AsEnumerable() // continue on memory
                .Select(x => new DateOnly(x.Year, x.Month, 1));
            return list;
        }

        public ShiftResult GetAllUnitShifts(DateOnly? month = null, params int[] unitFilter)
        {
            var targetMonth = month ?? DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz));
            var shiftMeta = shiftUOW.ShiftMeta.Find(x => x.Month.Year == targetMonth.Year && x.Month.Month == targetMonth.Month);

            var allUsers = userInfoService.GetNurseList(unitFilter);

            var slots = shiftUOW.ShiftSlot.GetAll().Where(x => x.Date.Year == targetMonth.Year && x.Date.Month == targetMonth.Month).ToList();
            var userShifts = shiftUOW.UserShift.GetAll(false).Where(x => x.Month.Year == targetMonth.Year && x.Month.Month == targetMonth.Month).ToList();

            var users = allUsers.GroupJoin(userShifts, x => x.User.Id, y => y.UserId, (x, y) => y.DefaultIfEmpty(new UserShift { UserId = x.User.Id, Suspended = false }).First());
            var result = shiftProcessor.ProcessSlotData(users, slots);
            return result;
        }

        public ShiftResult GetShiftForUnit(int unitId, DateOnly? month = null)
        {
            var targetMonth = month ?? DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz));
            var shiftMeta = shiftUOW.ShiftMeta.Find(x => x.Month.Year == targetMonth.Year && x.Month.Month == targetMonth.Month && x.ScheduleMeta.UnitId == unitId).FirstOrDefault();

            var allUsers = userInfoService.GetNurseList(new[] { unitId });

            var userIdList = allUsers.Select(x => x.User.Id).ToList();
            var slots = shiftUOW.ShiftSlot.GetAll().Where(x => x.Date.Year == targetMonth.Year && x.Date.Month == targetMonth.Month && userIdList.Contains(x.UserId)).ToList();
            var userShifts = shiftUOW.UserShift.GetAll(false).Where(x => x.Month.Year == targetMonth.Year && x.Month.Month == targetMonth.Month && userIdList.Contains(x.UserId)).ToList();

            var users = allUsers.GroupJoin(userShifts, x => x.User.Id, y => y.UserId, (x, y) => y.DefaultIfEmpty(new UserShift { UserId = x.User.Id, Suspended = false }).First());
            var result = shiftProcessor.ProcessSlotData(users, slots);
            return result;
        }

        public UserShiftResult GetShiftForUser(Guid userId, DateOnly? month = null)
        {
            var targetMonth = month ?? DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz));

            var slots = shiftUOW.ShiftSlot.Find(x => x.Date.Year == targetMonth.Year && x.Date.Month == targetMonth.Month && x.UserId == userId);
            var userShift = shiftUOW.UserShift.Find(x => x.Month.Year == targetMonth.Year && x.Month.Month == targetMonth.Month && x.UserId == userId).FirstOrDefault();

            return new UserShiftResult
            {
                UserShift = userShift,
                Slots = slots
            };
        }

        public IEnumerable<ShiftIncharge> GetInchargeList(int unitId, DateOnly? month = null)
        {
            var query = shiftUOW.ShiftIncharge.Find(x => x.UnitId == unitId);
            if (month != null)
            {
                query = query.Where(x => x.Date.Year == month.Value.Year && x.Date.Month == month.Value.Month);
            }
            var result = query.OrderBy(x => x.Date);
            return result;
        }

        public bool ClearShiftHistory(DateOnly? month = null)
        {
            if (!month.HasValue)
            {
                month = DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(DateTime.UtcNow, tz));
            }
            Expression<Func<ShiftSlot, bool>> filter =
                (ShiftSlot x) => x.Date.Year < month.Value.Year || (x.Date.Year == month.Value.Year && x.Date.Month < month.Value.Month);
            var records = shiftUOW.ShiftSlot.Find(filter, false).ToList();
            shiftUOW.ShiftSlot.DeleteRange(records);

            return shiftUOW.ShiftSlot.Complete() > 0;
        }
    }
}
