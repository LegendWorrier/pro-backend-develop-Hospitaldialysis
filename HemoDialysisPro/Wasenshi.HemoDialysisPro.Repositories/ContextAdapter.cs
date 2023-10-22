using Microsoft.EntityFrameworkCore;
using System.Linq;
using Wasenshi.HemoDialysisPro.Models.Interfaces;
using Wasenshi.HemoDialysisPro.Repository.Core.Interfaces.Base;

namespace Wasenshi.HemoDialysisPro.Repositories
{
    public class ContextAdapterAspNet : IContextAdapter
    {
        private readonly ApplicationDbContext context;

        public ContextAdapterAspNet(ApplicationDbContext context)
        {
            this.context = context;
        }

        public IQueryable<IUser> GetUsers()
        {
            return context.Users.Include(x => x.Units).AsSplitQuery();
        }

        public IQueryable<IRole> GetRoles()
        {
            return context.Roles;
        }

        public IQueryable<IUserRole> GetUserRoles()
        {
            return context.UserRoles;
        }

        public IApplicationDbContext Context => context;
    }
}
