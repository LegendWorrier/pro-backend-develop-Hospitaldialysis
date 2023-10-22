using System;
using System.Collections.Generic;
using Wasenshi.HemoDialysisPro.ViewModels.Base;

namespace Wasenshi.HemoDialysisPro.ViewModels
{
    public class CreateMedHistoryBatchViewModel : EntityViewModel
    {
        public string PatientId { get; set; }
        public DateTimeOffset EntryTime { get; set; }

        public IEnumerable<MedInfoViewModel> Meds { get; set; }
    }

    public class MedInfoViewModel
    {
        public DateTimeOffset? EntryTime { get; set; }
        public int MedicineId { get; set; }
        public int Quantity { get; set; }

        public float? OverrideDose { get; set; }
        public string OverrideUnit { get; set; }
    }
}
