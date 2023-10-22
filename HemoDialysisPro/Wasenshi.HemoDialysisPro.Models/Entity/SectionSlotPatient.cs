using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Wasenshi.HemoDialysisPro.Models.Enums;

namespace Wasenshi.HemoDialysisPro.Models
{
    public class SectionSlotPatient : EntityBase
    {
        [Key]
        public int SectionId { get; set; }
        [Key]
        public string PatientId { get; set; }
        [Key]
        public SectionSlots Slot { get; set; }

        [NotMapped]
        public Patient Patient { get; set; }
        [NotMapped]
        public ScheduleSection Section { get; set; }
    }
}
