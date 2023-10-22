using System.Collections.Generic;

namespace Wasenshi.HemoDialysisPro.ViewModels
{
    public class UserShiftViewModel : UserShiftEditViewModel
    {
        public IEnumerable<ShiftSlotViewModel> ShiftSlots { get; set; }
    }
}
