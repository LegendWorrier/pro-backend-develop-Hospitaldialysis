using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Wasenshi.HemoDialysisPro.Models.Entity.Stockable;

namespace Wasenshi.HemoDialysisPro.Models
{
    public class MedOverview
    {
        public string PatientId { get; set; }

        public IEnumerable<MedItem> ThisMonthMeds { get; set; }
    }

    public class MedItem
    {
        public int MedId { get; set; }
        public int Count { get; set; }

        [NotMapped]
        public Medicine Medicine { get; set; }
    }
}