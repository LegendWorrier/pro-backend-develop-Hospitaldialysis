using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Wasenshi.HemoDialysisPro.Models
{
    public class ProgressNote : EntityBase<Guid>
    {
        [Required]
        public Guid HemodialysisId { get; set; }
        [Required]
        public short Order { get; set; } // ordering

        [Required]
        public string Focus { get; set; }

        public string A { get; set; } // Assessment
        public string I { get; set; } // Intervention
        public string E { get; set; } // Evaluation


        [NotMapped]
        public HemodialysisRecord Hemodialysis { get; set; }
    }
}
