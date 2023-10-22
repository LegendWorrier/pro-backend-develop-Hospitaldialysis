using Wasenshi.HemoDialysisPro.Repositories.Interfaces;
using Wasenshi.HemoDialysisPro.Repositories.Repository;
using Wasenshi.HemoDialysisPro.Repository.Core.Interfaces.Base;

namespace Wasenshi.HemoDialysisPro.JobsServer.Implementation
{
    public interface IRoleRepository : IRoleRepository<RoleImp>
    {
    }

    public class RoleRepository : RoleRepository<RoleImp>, IRoleRepository
    {
        public RoleRepository(IContextAdapter context) : base(context)
        {
        }
    }
}
