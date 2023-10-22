using System.ComponentModel.DataAnnotations;
using Wasenshi.HemoDialysisPro.Models.Enums;

namespace Wasenshi.HemoDialysisPro.Models.Entity.Stockable
{
    // Master Data
    public class Dialyzer : Stockable
    {
        public string BrandName { get; set; }
        public DialyzerType Flux { get; set; }
        public MembraneType Membrane { get; set; }

        public float? SurfaceArea { get; set; } // m^2

    }

    public class DialyzerStock : StockItem<Dialyzer>
    {
    }
}
