using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Wasenshi.HemoDialysisPro.Models
{
    [Owned]
    public class VitalSignRecord
    {
        [Key]
        public long Id { get; set; }
        public DateTime Timestamp { get; set; }
        public int BPS { get; set; } // Blood Pressure / up
        public int BPD { get; set; } // Blood Pressure / down
        public int HR { get; set; } // Heart rate
        public int RR { get; set; } // Respiration rate
        public float Temp { get; set; } // temperature
        public float SpO2 { get; set; } // spO2
        public Postures? Posture { get; set; }

        public Guid HemodialysisRecordId { get; set; }

        public enum Postures
        {
            Lying,
            Sitting,
            Standing
        }
    }

    public class VitalSignComparer : IEqualityComparer<VitalSignRecord>
    {
        public bool Equals(VitalSignRecord x, VitalSignRecord y)
        {
            if (x == null || y == null)
            {
                return x == y;
            }
            return x.Id == y.Id;
        }

        public int GetHashCode([DisallowNull] VitalSignRecord obj)
        {
            return obj.Id.GetHashCode();
        }
    }
}