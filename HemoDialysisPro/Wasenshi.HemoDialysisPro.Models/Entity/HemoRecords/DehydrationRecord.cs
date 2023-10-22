using Microsoft.EntityFrameworkCore;
using System;

namespace Wasenshi.HemoDialysisPro.Models
{
    [Owned]
    public class DehydrationRecord
    {
        public float LastPostWeight { get; set; } // Last Post-Dialysis Weight
        public DateTime? CheckInTime { get; set; }
        public float PreTotalWeight { get; set; }
        public float WheelchairWeight { get; set; }
        public float ClothWeight { get; set; }
        public float FoodDrinkWeight { get; set; } // kg
        public float? BloodTransfusion { get; set; } // ml
        public float? ExtraFluid { get; set; } // ml
        public float UFGoal { get; set; } // L
        public float PostTotalWeight { get; set; }
        public float PostWheelchairWeight { get; set; }
        // ========== Abnormal Weight ==============
        public bool Abnormal { get; set; }
        public string Reason { get; set; }
    }
}