using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wasenshi.HemoDialysisPro.ViewModels.Stock
{
    public class StockItemBulkViewModel
    {
        public IEnumerable<StockItemViewModel> Data { get; set; }
        public IEnumerable<Guid> RemoveList { get; set; }
    }
}
