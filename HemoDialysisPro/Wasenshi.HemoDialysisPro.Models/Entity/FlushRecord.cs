using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Wasenshi.HemoDialysisPro.Models
{
    public class FlushRecord : ExecutionRecord
    {
        [Required]
        public Guid RecordId { get; set; }

        [NotMapped] public DialysisRecord Record { get; set; }
    }
}
