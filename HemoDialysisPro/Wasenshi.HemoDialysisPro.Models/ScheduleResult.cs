using System.Collections.Generic;

namespace Wasenshi.HemoDialysisPro.Models
{
    public class ScheduleResult
    {
        public int UnitId { get; set; }

        public IEnumerable<SectionResult> Sections { get; set; }
    }

    public class SectionResult
    {
        public ScheduleSection Section { get; set; }

        public IEnumerable<ScheduleSlot> Slots { get; set; }
    }
}
