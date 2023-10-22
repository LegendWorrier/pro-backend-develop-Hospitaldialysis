using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Wasenshi.HemoDialysisPro.Models
{
    public class Admission : EntityBase<Guid>
    {
        [Required]
        public string AN { get; set; }
        [Required]
        public string PatientId { get; set; }

        public DateTime Admit { get; set; }
        public DateTime? Discharged { get; set; }


        public string ChiefComplaint { get; set; } // อาการสำคัญ (ที่มา admit)
        public string Diagnosis { get; set; }

        public string Room { get; set; } // ตึกที่พักอยู่
        public string TelNo { get; set; } // เบอร์ติดต่อตึก

        public string StatusDc { get; set; } // status ตอน discharged
        public string TransferTo { get; set; }

        public ICollection<AdmissionUnderlying> Underlying { get; set; } // โรคประจำตัว/โรคแทรกซ้อน
    }

    public class AdmissionUnderlying : IEqualityComparer<AdmissionUnderlying>
    {
        public Guid AdmissionId { get; set; }
        public int UnderlyingId { get; set; }

        [NotMapped, JsonIgnore]
        public Admission Admission { get; set; }
        [NotMapped, JsonIgnore]
        public Underlying Underlying { get; set; }

        public bool Equals(AdmissionUnderlying x, AdmissionUnderlying y)
        {
            if (x == null || y == null)
            {
                return x == y;
            }
            return x.AdmissionId.Equals(y.AdmissionId) && x.UnderlyingId == y.UnderlyingId;
        }

        public int GetHashCode([DisallowNull] AdmissionUnderlying obj)
        {
            return obj.UnderlyingId.GetHashCode() ^ obj.AdmissionId.GetHashCode();
        }
    }
}
