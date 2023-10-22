using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Wasenshi.HemoDialysisPro.Models.MappingModels;
using Wasenshi.HemoDialysisPro.Models.Enums;

namespace Wasenshi.HemoDialysisPro.Models.Entity.Stockable
{
    // Master Data
    public class Medicine : Stockable
    {
        public int? CategoryId { get; set; }
        public UsageWays UsageWays { get; set; }

        public float? Dose { get; set; }

        public MedicineType MedType { get; set; } // used for TRT mapping

        // ========================== Relations ===============================

        [NotMapped]
        public MedCategory Category { get; set; }

        [NotMapped, JsonIgnore]
        public ICollection<Allergy> Allergies { get; set; }
    }

    public class MedicineStock : StockItem<Medicine>
    {
    }
}