using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Repositories.Interfaces;
using Wasenshi.HemoDialysisPro.Repository.Core.Interfaces.Base;

namespace Wasenshi.HemoDialysisPro.Repositories.Repository
{
    public class UnitRepository : Repository<Unit>, IUnitRepository
    {
        public UnitRepository(IContextAdapter context) : base(context)
        {
        }
    }
}
