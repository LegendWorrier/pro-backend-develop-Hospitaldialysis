using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Wasenshi.HemoDialysisPro.Models.Enums;

namespace Wasenshi.HemoDialysisPro.Models
{
    public abstract class ExecutionRecord : EntityBase<Guid>
    {
        [Required]
        public Guid HemodialysisId { get; set; }
        [Required]
        public DateTime Timestamp { get; set; }

        public ExecutionType Type { get; set; }

        public bool IsExecuted { get; set; }

        public Guid? CoSign { get; set; }

        [NotMapped] public HemodialysisRecord Hemodialysis { get; set; }
    }
}
