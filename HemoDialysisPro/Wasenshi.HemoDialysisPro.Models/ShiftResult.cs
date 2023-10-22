using System.Collections.Generic;

namespace Wasenshi.HemoDialysisPro.Models
{
    public class ShiftResult
    {
        public IEnumerable<UserShiftResult> Users { get; set; }
    }

    public class UserShiftResult
    {
        public UserShift UserShift { get; set; }

        public IEnumerable<ShiftSlot> Slots { get; set; }
    }
}
