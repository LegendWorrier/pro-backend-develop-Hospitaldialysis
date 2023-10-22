using Wasenshi.HemoDialysisPro.Models.Entity.Stockable;

namespace Wasenshi.HemoDialysisPro.Web.Api.Utils.ModelSearchers
{
    public class StockSearcher<T> : StockBaseSearcher<T> where T : Stockable
    {
        public StockSearcher() : base(x => x)
        {
        }
    }
}
