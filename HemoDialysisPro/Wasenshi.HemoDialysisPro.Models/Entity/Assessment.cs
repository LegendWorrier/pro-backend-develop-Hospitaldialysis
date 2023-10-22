using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Wasenshi.HemoDialysisPro.Models.Enums;

namespace Wasenshi.HemoDialysisPro.Models
{
    public class Assessment : EntityBase<long>
    {
        public AssessmentTypes Type { get; set; }
        public int? GroupId { get; set; }
        public int Order { get; set; }
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public OptionTypes OptionType { get; set; }
        public bool Multi { get; set; } // For checkbox only or text/number where choices are available

        // ----------- For Extra information ---------------
        public bool HasOther { get; set; }
        public bool HasText { get; set; }
        public bool HasNumber { get; set; }

        public string Note { get; set; }

        [NotMapped]
        public ICollection<AssessmentOption> OptionsList { get; set; }
        [NotMapped]
        public AssessmentGroup Group { get; set; }
    }
}
