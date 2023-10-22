using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wasenshi.HemoDialysisPro.Models.Entity.Accounting
{
    public enum StockType
    {
        NORMAL, // = รายการธรรมดา
        BF, // Brought Forward = ยอดยกมา
        ADJUST // ปรับปรุงรายการ / นับใหม่
    }
}
