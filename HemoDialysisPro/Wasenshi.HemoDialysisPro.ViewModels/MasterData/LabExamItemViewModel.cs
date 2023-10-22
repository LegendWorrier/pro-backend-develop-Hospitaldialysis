using Wasenshi.HemoDialysisPro.Models.Enums;

namespace Wasenshi.HemoDialysisPro.ViewModels
{
    public class LabExamItemViewModel : MasterDataViewModel
    {
        public string Unit { get; set; }
        public LabCategory Category { get; set; }
        public bool IsYesNo { get; set; }

        public float? UpperLimit { get; set; } // Default
        public float? LowerLimit { get; set; } // Default
        public float? UpperLimitM { get; set; } // Male
        public float? LowerLimitM { get; set; } // Male
        public float? UpperLimitF { get; set; } // Female
        public float? LowerLimitF { get; set; } // Female

        public TRTMappingLab TRT { get; set; }


        // =========== read only ==================
        public bool IsSystemBound { get; set; }
        public string Bound { get; set; }

        public bool IsCalculated { get; set; }
    }
}
