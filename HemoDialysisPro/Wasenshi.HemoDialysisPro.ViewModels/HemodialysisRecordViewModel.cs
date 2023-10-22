using System;
using System.Collections.Generic;
using Wasenshi.HemoDialysisPro.Models.Enums;
using Wasenshi.HemoDialysisPro.ViewModels.Base;

namespace Wasenshi.HemoDialysisPro.ViewModels
{
    public class HemodialysisRecordViewModel : EntityViewModel
    {
        public Guid Id { get; set; }
        public DateTimeOffset? CompletedTime { get; set; }
        public string PatientId { get; set; }

        public string Admission { get; set; }
        public bool? OutsideUnit { get; set; }
        public string Ward { get; set; }
        public string Bed { get; set; }
        public DateTimeOffset? CycleStartTime { get; set; }
        public DateTimeOffset? CycleEndTime { get; set; }

        public bool IsICU { get; set; }
        public DialysisType Type { get; set; }

        public bool AcNotUsed { get; set; }
        public string ReasonForRefraining { get; set; }

        // Planning (actual amount will be calculated from dialysis record)
        public float? FlushNSS { get; set; } // ml
        public int? FlushNSSInterval { get; set; } // interval in minutes (min)
        public int? FlushTimes { get; set; } // Flush NSS multiplied by this = Total amount (ml)

        public DehydrationRecordViewModel Dehydration { get; set; }
        public DialysisPrescriptionViewModel DialysisPrescription { get; set; } = null;
        public ICollection<VitalSignRecordViewModel> PreVitalsign { get; set; }
        public ICollection<VitalSignRecordViewModel> PostVitalsign { get; set; }
        public DialyzerRecordViewModel Dialyzer { get; set; }
        public BloodCollectionRecordViewModel BloodCollection { get; set; }
        public AVShuntRecordViewModel AvShunt { get; set; }

        public Guid? ProofReader { get; set; }
        public bool DoctorConsent { get; set; }
        public Guid? DoctorId { get; set; }
        public Guid[] NursesInShift { get; set; }

        public bool SentPDF { get; set; }

        public HemoNoteViewModel Note { get; set; }
    }

    public class DehydrationRecordViewModel
    {
        public float? LastPostWeight { get; set; } // Last Post-Dialysis Weight
        public DateTimeOffset? CheckInTime { get; set; }
        public float? PreTotalWeight { get; set; }
        public float? WheelchairWeight { get; set; }
        public float? ClothWeight { get; set; }
        public float? FoodDrinkWeight { get; set; }
        public float? ExtraFluid { get; set; }
        public float? BloodTransfusion { get; set; }
        public float? UFGoal { get; set; }
        public float? PostTotalWeight { get; set; }
        public float? PostWheelchairWeight { get; set; }
        // ========== Abnormal Weight ==============
        public bool Abnormal { get; set; }
        public string Reason { get; set; }
    }

    public class DialyzerRecordViewModel
    {
        public int? UseNo { get; set; }
        public float? TCV { get; set; } // % by default
    }

    public class BloodCollectionRecordViewModel
    {
        public string Pre { get; set; }
        public string Post { get; set; }
    }

    public class VitalSignRecordViewModel
    {
        public DateTimeOffset Timestamp { get; set; }
        public int BPS { get; set; } // Blood Pressure / up
        public int BPD { get; set; } // Blood Pressure / down
        public int HR { get; set; } // Heart rate
        public int RR { get; set; } // Respiration rate
        public float Temp { get; set; } // temperature
        public float SpO2 { get; set; } // spO2
        public Postures? Posture { get; set; }

        public enum Postures
        {
            Lying,
            Sitting,
            Standing
        }
    }

    public class AVShuntRecordViewModel
    {
        public Guid? AVShuntId { get; set; }
        public string ShuntSite { get; set; }

        // ============= Non-AV ============================
        public float? ALength { get; set; } // cm
        public float? VLength { get; set; } // cm

        public float? ANeedleCC { get; set; }
        public float? VNeedleCC { get; set; }

        // ================= AV ====================
        public int? ASize { get; set; }
        public int? VSize { get; set; }

        public int? ANeedleTimes { get; set; }
        public int? VNeedleTimes { get; set; }
    }
}
