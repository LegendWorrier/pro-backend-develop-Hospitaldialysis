using System;
using Wasenshi.HemoDialysisPro.Models.Enums;
using Wasenshi.HemoDialysisPro.ViewModels.Base;

namespace Wasenshi.HemoDialysisPro.ViewModels
{
    public class ScheduleViewModel : EntityViewModel
    {
        public Guid Id { get; set; }
        public string PatientId { get; set; }
        public int SectionId { get; set; }
        public SectionSlots Slot { get; set; }

        // override
        public DateTimeOffset Date { get; set; }
        public int? OverrideUnitId { get; set; }
        public DateTimeOffset? OriginalDate { get; set; }
    }
}
