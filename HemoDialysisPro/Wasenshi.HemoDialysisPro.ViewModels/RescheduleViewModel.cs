using System;

namespace Wasenshi.HemoDialysisPro.ViewModels
{
    public class RescheduleViewModel
    {
        public DateTimeOffset Date { get; set; }
        public int? OverrideUnitId { get; set; }
        public DateTimeOffset? OriginalDate { get; set; }
        public string TargetPatientId { get; set; }
    }
}
