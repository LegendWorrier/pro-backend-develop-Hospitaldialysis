using System;
using Wasenshi.HemoDialysisPro.ViewModels.Base;

namespace Wasenshi.HemoDialysisPro.ViewModels
{
    public class PatientHistoryViewModel : EntityViewModel
    {
        public Guid Id { get; set; }
        public string PatientId { get; set; }
        public int HistoryItemId { get; set; }

        public float? NumberValue { get; set; }
        public string Value { get; set; }

        public PatientHistoryItemViewModel HistoryItem { get; set; }
    }
}
