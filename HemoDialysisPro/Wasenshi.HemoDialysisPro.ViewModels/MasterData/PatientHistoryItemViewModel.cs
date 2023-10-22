using System.Collections.Generic;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Models.Enums;

namespace Wasenshi.HemoDialysisPro.ViewModels
{
    public class PatientHistoryItemViewModel : MasterDataViewModel
    {
        public string DisplayName { get; set; }
        public int Order { get; set; }

        public bool IsYesNo { get; set; }
        public bool IsNumber { get; set; }

        public IEnumerable<PatientChoiceViewModel> Choices { get; set; }
        public bool AllowOther { get; set; } // whether this item can have value other than listed by choices

        public TRTMappingPatient TRT { get; set; }
    }

    public class PatientChoiceViewModel
    {
        public string Text { get; set; }
        public float? NumberValue { get; set; }
    }
}
