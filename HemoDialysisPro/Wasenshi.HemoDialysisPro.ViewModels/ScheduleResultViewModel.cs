using System.Collections.Generic;

namespace Wasenshi.HemoDialysisPro.ViewModels
{
    public class ScheduleResultViewModel
    {
        public int UnitId { get; set; }
        public IEnumerable<SectionResultViewModel> Sections { get; set; }
        public IEnumerable<ScheduleViewModel> Reschedules { get; set; }
        public IEnumerable<PatientViewModel> Patients { get; set; }
    }

    public class SectionResultViewModel
    {
        public ScheduleSectionViewModel Section { get; set; }

        public IEnumerable<ScheduleSlotViewModel> Slots { get; set; }
    }
}
