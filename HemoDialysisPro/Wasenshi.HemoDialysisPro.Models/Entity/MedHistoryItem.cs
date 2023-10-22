using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Wasenshi.HemoDialysisPro.Models.Entity.Stockable;

namespace Wasenshi.HemoDialysisPro.Models
{
    public class MedHistoryItem : EntityBase<Guid>
    {
        [Required]
        public DateTime EntryTime { get; set; }
        [Required]
        public string PatientId { get; set; }
        [Required]
        public int MedicineId { get; set; }

        public int Quantity { get; set; }

        public float? OverrideDose { get; set; }
        public string OverrideUnit { get; set; }


        [NotMapped] public Medicine Medicine { get; set; }
    }
}
