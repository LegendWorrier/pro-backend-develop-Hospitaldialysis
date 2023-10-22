using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Wasenshi.HemoDialysisPro.Models.Enums;

namespace Wasenshi.HemoDialysisPro.Models
{
    public class ScheduleSlot
    {
        public int SectionId { get; set; }
        public SectionSlots Slot { get; set; }

        [NotMapped]
        public ScheduleSection Section { get; set; }

        [NotMapped]
        public IEnumerable<SectionSlotPatient> PatientList { get; set; }
    }
}
