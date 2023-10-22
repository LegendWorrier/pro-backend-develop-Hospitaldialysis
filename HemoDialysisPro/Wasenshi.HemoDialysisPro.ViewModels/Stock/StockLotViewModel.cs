using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wasenshi.HemoDialysisPro.ViewModels.Stock
{
    public class StockLotViewModel
    {
        public IEnumerable<StockItemWithTypeViewModel> Data { get; set; }
    }
}
