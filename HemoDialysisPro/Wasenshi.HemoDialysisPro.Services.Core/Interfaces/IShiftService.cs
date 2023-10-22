using System;
using System.Collections.Generic;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Services.Interfaces.Base;

namespace Wasenshi.HemoDialysisPro.Services.Interfaces
{
    public interface IShiftService : IApplicationService
    {
        bool hasIncharge(Guid userId, IEnumerable<ScheduleSection> scheduleSections);
        bool IsIncharge(Guid userId, int unitId, ScheduleSection overriedSection = null);
        bool AddOrUpdateIncharge(IEnumerable<ShiftIncharge> incharges);
        bool ClearIncharge(DateOnly? month = null);
        IEnumerable<ShiftIncharge> GetInchargeList(int unitId, DateOnly? month = null);

        IEnumerable<ShiftSlot> CreateOrUpdateShift(DateOnly month, IEnumerable<ShiftSlot> slots, IEnumerable<UserShift> suspendList);
        IEnumerable<UserShift> SuspendUser(IEnumerable<UserShift> suspendList, DateOnly month);

        ShiftResult GetAllUnitShifts(DateOnly? month = null, params int[] unitFilter);
        ShiftResult GetShiftForUnit(int unitId, DateOnly? month = null);
        UserShiftResult GetShiftForUser(Guid user, DateOnly? month = null);

        IEnumerable<DateOnly> GetHistoryList(IEnumerable<int> unitFilter = null);
        bool ClearShiftHistory(DateOnly? month = null);
    }
}