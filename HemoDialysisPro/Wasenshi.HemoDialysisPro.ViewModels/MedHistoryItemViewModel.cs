using System;
using Wasenshi.HemoDialysisPro.ViewModels.Base;

namespace Wasenshi.HemoDialysisPro.ViewModels
{
    public class MedHistoryItemViewModel : EntityViewModel
    {
        public Guid Id { get; set; }
        public DateTimeOffset EntryTime { get; set; }
        public string PatientId { get; set; }

        public int MedicineId { get; set; }
        public MedicineViewModel Medicine { get; set; }

        public int Quantity { get; set; }

        public float? OverrideDose { get; set; }
        public string OverrideUnit { get; set; }
    }
}