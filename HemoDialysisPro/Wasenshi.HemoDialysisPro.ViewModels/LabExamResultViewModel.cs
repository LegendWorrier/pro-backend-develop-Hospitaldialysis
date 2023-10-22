using System;
using System.Collections.Generic;

namespace Wasenshi.HemoDialysisPro.ViewModels
{
    public class LabExamResultViewModel
    {
        public IEnumerable<DateTimeOffset> Columns { get; set; }
        public IEnumerable<KeyValuePair<LabExamItemViewModel, List<LabExamViewModel>[]>> Data { get; set; }
    }
}
