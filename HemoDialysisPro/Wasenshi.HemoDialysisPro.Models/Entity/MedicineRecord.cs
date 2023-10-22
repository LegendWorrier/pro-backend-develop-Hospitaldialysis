using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Wasenshi.HemoDialysisPro.Models.Enums;

namespace Wasenshi.HemoDialysisPro.Models
{
    public class MedicineRecord : ExecutionRecord
    {
        [Required]
        public Guid PrescriptionId { get; set; }

        public UsageWays? OverrideRoute { get; set; }
        public float? OverrideDose { get; set; }

        [NotMapped] public MedicinePrescription Prescription { get; set; }
    }
}
