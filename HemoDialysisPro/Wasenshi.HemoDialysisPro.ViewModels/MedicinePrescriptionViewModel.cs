using System;
using Wasenshi.HemoDialysisPro.Models.Enums;
using Wasenshi.HemoDialysisPro.ViewModels.Base;

namespace Wasenshi.HemoDialysisPro.ViewModels
{
    public class MedicinePrescriptionViewModel : EntityViewModel
    {
        public Guid Id { get; set; }
        public string PatientId { get; set; }

        public int MedicineId { get; set; }
        public MedicineViewModel Medicine { get; set; }

        public int Quantity { get; set; }
        public UsageWays Route { get; set; }
        public Frequency Frequency { get; set; }

        public DateTimeOffset AdministerDate { get; set; } // วันที่สั่งยา

        public int Duration { get; set; } // 0 = No expiration date

        public string HospitalName { get; set; } // in case of Medicine from Outside

        public float? OverrideDose { get; set; }
        public string OverrideUnit { get; set; }

        public string Note { get; set; }

        // ============ For FE ==============
        public bool IsHistory { get; set; }
    }
}