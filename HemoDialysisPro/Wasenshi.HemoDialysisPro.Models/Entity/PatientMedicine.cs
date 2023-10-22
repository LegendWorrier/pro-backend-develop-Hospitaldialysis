using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using Wasenshi.HemoDialysisPro.Models.Entity.Stockable;

namespace Wasenshi.HemoDialysisPro.Models
{
    public abstract class PatientMedicine
    {
        public string PatientId { get; set; }
        public int MedicineId { get; set; }

        [NotMapped, JsonIgnore]
        public Patient Patient { get; set; }
        [NotMapped, JsonIgnore]
        public Medicine Medicine { get; set; }
    }

    public class PatientMedicineComparer<TConcrete> : IEqualityComparer<TConcrete> where TConcrete : PatientMedicine
    {
        public bool Equals(TConcrete x, TConcrete y)
        {
            if (x == null || y == null)
            {
                return x == y;
            }
            return x.PatientId == y.PatientId && x.MedicineId == y.MedicineId;
        }

        public int GetHashCode([DisallowNull] TConcrete obj)
        {
            return (obj.PatientId?.GetHashCode() ?? "".GetHashCode()) ^ obj.MedicineId.GetHashCode();
        }
    }
}
