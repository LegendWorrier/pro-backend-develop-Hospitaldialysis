using System.Collections.Generic;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Services.Interfaces.Base;

namespace Wasenshi.HemoDialysisPro.Services.Interfaces
{
    public interface IShiftProcessor : IApplicationService
    {
        ShiftResult ProcessSlotData(IEnumerable<UserShift> users, IEnumerable<ShiftSlot> slots);
    }
}
