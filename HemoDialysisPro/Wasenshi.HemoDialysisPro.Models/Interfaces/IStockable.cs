using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wasenshi.HemoDialysisPro.Models.Interfaces
{
    public interface IStockable : IEntityBase<int>
    {
        public string Name { get; set; }
        public string Code { get; set; }
        public string Note { get; set; }
    }
}
