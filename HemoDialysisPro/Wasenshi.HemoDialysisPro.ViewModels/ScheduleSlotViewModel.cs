using System.Collections.Generic;
using Wasenshi.HemoDialysisPro.Models.Enums;
using Wasenshi.HemoDialysisPro.ViewModels.Base;

namespace Wasenshi.HemoDialysisPro.ViewModels
{
    public class ScheduleSlotViewModel
    {
        public int SectionId { get; set; }
        public int SectionStartTime { get; set; }
        public SectionSlots Slot { get; set; }

        public IEnumerable<PatientSlotViewModel> PatientList { get; set; } // map with Id
    }

    public class PatientSlotViewModel : EntityViewModel
    {
        public string PatientId { get; set; }
    }
}
