using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Wasenshi.HemoDialysisPro.Models
{
    public class LabOverview
    {
        [NotMapped]
        public Patient Patient { get; set; }

        public string PatientId { get; set; }
        public int Total { get; set; }
        public DateTime? LastRecord { get; set; }
    }
}