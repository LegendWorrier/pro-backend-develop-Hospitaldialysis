using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Repositories.Interfaces;
using Wasenshi.HemoDialysisPro.Repository.Core.Interfaces.Base;

namespace Wasenshi.HemoDialysisPro.Repositories.Repository
{
    public class DeathCauseRepository : Repository<DeathCause>, IDeathCauseRepository
    {
        public DeathCauseRepository(IContextAdapter context) : base(context)
        {
        }
    }
}
