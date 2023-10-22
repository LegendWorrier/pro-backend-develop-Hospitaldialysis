using System;
using System.Collections.Generic;

namespace Wasenshi.HemoDialysisPro.ViewModels
{
    public class UpdateUnitSectionViewModel
    {
        public IEnumerable<ScheduleSectionViewModel> SectionList { get; set; }
        public IEnumerable<int> DeleteList { get; set; }
        public DateTimeOffset? TargetEffectiveDate { get; set; }
    }
}
