using System;

namespace Wasenshi.HemoDialysisPro.ViewModels
{
    public class LabOverviewViewModel
    {
        public PatientViewModel Patient { get; set; }
        public int Total { get; set; }
        public DateTimeOffset? LastRecord { get; set; }
    }
}
