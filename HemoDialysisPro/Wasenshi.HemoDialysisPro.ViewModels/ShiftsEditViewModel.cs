using System.Collections.Generic;

namespace Wasenshi.HemoDialysisPro.ViewModels
{
    public class ShiftsEditViewModel
    {
        public IEnumerable<ShiftSlotViewModel> ShiftSlots { get; set; }
        public IEnumerable<UserShiftEditViewModel> SuspendedList { get; set; }
        /// <summary>
        /// For Self-Edit case.
        /// </summary>
        public bool? IsSuspended { get; set; } // for self edit case
    }
}
