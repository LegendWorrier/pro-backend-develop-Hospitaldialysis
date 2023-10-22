using Wasenshi.HemoDialysisPro.Models.Enums;

namespace Wasenshi.HemoDialysisPro.ViewModels
{
    public class DialyzerViewModel : StockableViewModel
    {
        public string BrandName { get; set; }
        public DialyzerType Flux { get; set; }
        public MembraneType Membrane { get; set; }
        public float? SurfaceArea { get; set; } // m^2
    }
}
