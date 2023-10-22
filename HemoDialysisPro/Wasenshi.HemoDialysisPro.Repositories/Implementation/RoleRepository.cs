using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Repositories.Interfaces;
using Wasenshi.HemoDialysisPro.Repositories.Repository;
using Wasenshi.HemoDialysisPro.Repository.Core.Interfaces.Base;

namespace Wasenshi.HemoDialysisPro.Repositories.Implementation
{
    public interface IRoleRepository : IRoleRepository<Role>
    {
    }

    public class RoleRepository : RoleRepository<Role>, IRoleRepository
    {
        public RoleRepository(IContextAdapter adapter) : base(adapter)
        {
        }
    }
}
