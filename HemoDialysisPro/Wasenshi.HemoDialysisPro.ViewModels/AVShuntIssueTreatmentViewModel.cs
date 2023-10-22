using System;
using Wasenshi.HemoDialysisPro.ViewModels.Base;

namespace Wasenshi.HemoDialysisPro.ViewModels
{
    public class AVShuntIssueTreatmentViewModel : EntityViewModel
    {
        public Guid Id { get; set; }
        public string PatientId { get; set; }
        public DateTimeOffset? AbnormalDatetime { get; set; }
        public string Complications { get; set; }
        public string TreatmentMethod { get; set; }

        public string Hospital { get; set; }
        public string TreatmentResult { get; set; }

        public Guid? CathId { get; set; }
    }
}