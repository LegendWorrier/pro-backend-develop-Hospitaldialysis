using System;
using Wasenshi.HemoDialysisPro.Models.Enums;

namespace Wasenshi.HemoDialysisPro.ViewModels
{
    public class SchedulePatientViewModel
    {
        public Guid Id { get; set; }
        public string PatientId { get; set; }
        public PatientViewModel Patient { get; set; }
        public ScheduleSectionViewModel OriginalSection { get; set; }
        public int OriginalSectionId { get; set; }
        public SectionSlots OriginalSlot { get; set; }

        public DateTimeOffset Date { get; set; }
        public int? OverrideUnitId { get; set; }
        public DateTimeOffset? OriginalDate { get; set; }
    }
}
