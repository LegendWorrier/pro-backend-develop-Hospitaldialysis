using System;
using System.Collections.Generic;

namespace Wasenshi.HemoDialysisPro.ViewModels
{
    public class ShiftResultViewModel
    {
        public int? UnitId { get; set; }
        public DateOnly Month { get; set; }
        public IEnumerable<UserShiftViewModel> Users { get; set; }
    }
}
