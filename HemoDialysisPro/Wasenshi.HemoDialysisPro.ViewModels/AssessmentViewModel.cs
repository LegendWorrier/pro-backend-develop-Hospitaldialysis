using System.Collections.Generic;
using Wasenshi.HemoDialysisPro.Models.Enums;
using Wasenshi.HemoDialysisPro.Models.Enums;
using Wasenshi.HemoDialysisPro.ViewModels.Base;

namespace Wasenshi.HemoDialysisPro.ViewModels
{
    public class AssessmentViewModel : EntityViewModel
    {
        public long Id { get; set; }
        public AssessmentTypes Type { get; set; }
        public int? GroupId { get; set; }
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public OptionTypes OptionType { get; set; }
        public bool Multi { get; set; } // For checkbox only


        // ----------- For Extra information ---------------
        public bool HasOther { get; set; }
        public bool HasText { get; set; }
        public bool HasNumber { get; set; }

        public string Note { get; set; }

        public ICollection<AssessmentOptionViewModel> OptionsList { get; set; }
    }
}
