using System.Collections.Generic;

namespace Wasenshi.HemoDialysisPro.Models
{
    public class InchargeResult
    {
        public int UnitId { get; set; }

        IEnumerable<ShiftIncharge> ShiftIncharges { get; set; }
    }
}
