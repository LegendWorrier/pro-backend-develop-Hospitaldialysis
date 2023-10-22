using System.Collections.Generic;

namespace Wasenshi.HemoDialysisPro.ViewModels
{
    public class GetSectionsViewModel
    {
        public IEnumerable<ScheduleSectionViewModel> Sections { get; set; }
        public IEnumerable<ScheduleSectionViewModel> Pendings { get; set; }
    }
}
