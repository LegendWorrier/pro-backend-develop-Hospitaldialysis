using System;
using System.Collections.Generic;

namespace Wasenshi.HemoDialysisPro.ViewModels
{
    public class MedHistoryResultViewModel
    {
        public IEnumerable<DateTimeOffset> Columns { get; set; }
        public IEnumerable<KeyValuePair<MedicineViewModel, List<MedHistoryItemViewModel>[]>> Data { get; set; }
    }
}
