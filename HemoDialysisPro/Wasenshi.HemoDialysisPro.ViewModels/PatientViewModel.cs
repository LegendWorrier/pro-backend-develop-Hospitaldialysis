using System;

namespace Wasenshi.HemoDialysisPro.ViewModels
{
    public class PatientViewModel : CreatePatientViewModel
    {
        public DateTimeOffset? Schedule { get; set; }
        public bool IsInSession { get; set; }
        public int TotalThisMonth { get; set; }
    }
}
