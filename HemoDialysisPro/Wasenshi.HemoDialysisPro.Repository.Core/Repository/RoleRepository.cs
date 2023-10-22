using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using Wasenshi.HemoDialysisPro.Models.Interfaces;
using Wasenshi.HemoDialysisPro.Repositories.Interfaces;
using Wasenshi.HemoDialysisPro.Repository.Core.Interfaces.Base;

namespace Wasenshi.HemoDialysisPro.Repositories.Repository
{
    public class RoleRepository<TRole> : Repository<TRole, Guid>, IRoleRepository<TRole> where TRole : class, IRole
    {
        private readonly IContextAdapter adapter;

        public RoleRepository(IContextAdapter adapter) : base(adapter)
        {
            this.adapter = adapter;
        }

        public IQueryable<string> GetUserRoles(Guid userId)
        {
            return GetUserRolesMap()
                        .Join(adapter.GetRoles().AsNoTracking(), ur => ur.RoleId, r => r.Id, (ur, r) =>
                        new { ur.UserId, Role = r.Name })
                        .Where(x => x.UserId == userId)
                        .Select(x => x.Role);
        }

        public IQueryable<IUserRole> GetUserRolesMap()
        {
            return adapter.GetUserRoles().AsNoTracking();
        }

        public Task<bool> IsInRoleAsync(IUser user, string role)
        {
            return adapter.GetUserRoles()
                .AsNoTracking()
                .Join(adapter.GetRoles().AsNoTracking(), x => x.RoleId, x => x.Id, (ur, r) => new { ur.UserId, Role = r.NormalizedName })
                .AnyAsync(x => x.UserId == user.Id && x.Role == role.ToUpper());
        }
    }
}
