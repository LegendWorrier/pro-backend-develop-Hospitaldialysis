using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Wasenshi.HemoDialysisPro.Models.Enums;

namespace Wasenshi.HemoDialysisPro.Models
{
    /// <summary>
    /// This data will override normal schedule in each section slot
    /// </summary>
    public class Schedule : EntityBase<Guid>
    {
        // Original
        public string PatientId { get; set; }
        public int SectionId { get; set; }
        [Required]
        public SectionSlots Slot { get; set; }

        // override
        [Required]
        public DateTime Date { get; set; }
        public int? OverrideUnitId { get; set; } // in case change unit
        public DateTime? OriginalDate { get; set; } // in case there is specific original point

        [NotMapped]
        public Patient Patient { get; set; }
        [NotMapped]
        public ScheduleSection Section { get; set; }
    }
}
