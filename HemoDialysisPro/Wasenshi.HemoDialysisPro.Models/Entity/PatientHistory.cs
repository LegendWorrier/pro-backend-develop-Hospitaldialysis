using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Wasenshi.HemoDialysisPro.Models
{
    [PrimaryKey(nameof(PatientId), nameof(HistoryItemId))]
    public class PatientHistory : EntityBase
    {
        [Key]
        public string PatientId { get; set; }
        [Key]
        public int HistoryItemId { get; set; }

        public float? NumberValue { get; set; }
        public string Value { get; set; }

        [NotMapped]
        public PatientHistoryItem HistoryItem { get; set; }
        [NotMapped]
        public Patient Patient { get; set; }
    }
}