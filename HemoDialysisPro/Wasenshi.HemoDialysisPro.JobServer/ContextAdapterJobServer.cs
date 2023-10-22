using Microsoft.EntityFrameworkCore;
using Wasenshi.HemoDialysisPro.Models.Interfaces;
using Wasenshi.HemoDialysisPro.Repositories;
using Wasenshi.HemoDialysisPro.Repository.Core.Interfaces.Base;

namespace Wasenshi.HemoDialysisPro.JobsServer
{
    public class ContextAdapterJobServer : IContextAdapter
    {
        private readonly ApplicationDbContextJobServer context;

        public ContextAdapterJobServer(ApplicationDbContextJobServer context)
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
