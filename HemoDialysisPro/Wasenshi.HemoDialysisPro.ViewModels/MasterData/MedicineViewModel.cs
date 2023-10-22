using Wasenshi.HemoDialysisPro.Models.Enums;

namespace Wasenshi.HemoDialysisPro.ViewModels
{
    public class MedicineViewModel : StockableViewModel
    {
        public int? CategoryId { get; set; }
        public string Category { get; set; }
        public UsageWays UsageWays { get; set; }

        public float? Dose { get; set; }

        public MedicineType MedType { get; set; }
    }
}
