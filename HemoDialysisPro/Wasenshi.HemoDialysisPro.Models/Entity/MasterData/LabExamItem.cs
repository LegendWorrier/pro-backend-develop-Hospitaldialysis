using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Models.Enums;

namespace Wasenshi.HemoDialysisPro.Models
{
    public class LabExamItem : EntityBase<int>
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string Unit { get; set; }

        public bool IsYesNo { get; set; }

        public LabCategory Category { get; set; } // every 1/3/6/12 months

        public float? UpperLimit { get; set; } // Default
        public float? LowerLimit { get; set; } // Default
        public float? UpperLimitM { get; set; } // Male
        public float? LowerLimitM { get; set; } // Male
        public float? UpperLimitF { get; set; } // Female
        public float? LowerLimitF { get; set; } // Female

        public TRTMappingLab TRT { get; set; }


        // ========= System Only ===================
        public bool IsSystemBound { get; set; } = false; // indicate that this item is special and is referenced, protected by system (cannot be deleted)
        [JsonConverter(typeof(JsonEnumConverter<SpecialLabItem?>))]
        public SpecialLabItem? Bound { get; set; } // indicate the special value that this item is tied to.
        public bool IsCalculated { get; set; } = false; // indicate that this item is calculated by the system, and cannot be modified by user.
    }
}