using Wasenshi.HemoDialysisPro.Models;

namespace Wasenshi.HemoDialysisPro.Share.GlobalEvents
{
    public class UnitUpdated
    {
        public Unit Data { get; set; }
        public bool Remove { get; set; }
    }
}
