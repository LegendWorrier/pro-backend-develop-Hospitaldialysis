using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Wasenshi.HemoDialysisPro.Models
{
    public class LabExam : EntityBase<Guid>
    {
        [Required]
        public DateTime EntryTime { get; set; }
        [Required]
        public string PatientId { get; set; }
        [Required]
        public int LabItemId { get; set; }
        public float LabValue { get; set; }

        public string Note { get; set; }

        [NotMapped]
        public LabExamItem LabItem { get; set; }
        [NotMapped]
        public Patient Patient { get; set; }
    }
}