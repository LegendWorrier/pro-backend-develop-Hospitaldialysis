using System;
using System.Linq;
using System.Threading.Tasks;
using Wasenshi.HemoDialysisPro.Models.Interfaces;
using Wasenshi.HemoDialysisPro.Repositories.Interfaces.Base;

namespace Wasenshi.HemoDialysisPro.Repositories.Interfaces
{
    public interface IRoleRepository<TRole> : IRepository<TRole, Guid> where TRole : class, IRole
    {
        IQueryable<IUserRole> GetUserRolesMap();
        IQueryable<string> GetUserRoles(Guid userId);
        Task<bool> IsInRoleAsync(IUser user, string role);
    }
}
