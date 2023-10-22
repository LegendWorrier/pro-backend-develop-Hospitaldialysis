using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Wasenshi.HemoDialysisPro.Models
{
    public class NurseRecord : EntityBase<Guid>
    {
        [Required]
        public Guid HemodialysisId { get; set; }
        [Required]
        public DateTime Timestamp { get; set; }
        [Required]
        public string Content { get; set; }

        [NotMapped]
        public HemodialysisRecord Hemodialysis { get; set; }
    }
}
