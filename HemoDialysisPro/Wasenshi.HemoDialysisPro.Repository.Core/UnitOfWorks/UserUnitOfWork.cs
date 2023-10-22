using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Wasenshi.HemoDialysisPro.Models.Interfaces;
using Wasenshi.HemoDialysisPro.Repositories.Interfaces;
using Wasenshi.HemoDialysisPro.Repositories.Repository;
using Wasenshi.HemoDialysisPro.Repository.Core.Interfaces.Base;

namespace Wasenshi.HemoDialysisPro.Repositories
{
    public class UserUnitOfWork<TUser, TRole> : IUserUnitOfWork<TUser, TRole> where TUser : class, IUser where TRole : class, IRole
    {
        private readonly IContextAdapter _context;
        private readonly IConfiguration _config;

        public IUserRepository<TUser> User { get; }
        public IRoleRepository<TRole> Role { get; }

        public UserUnitOfWork(IContextAdapter context, IConfiguration config)
        {
            _context = context;
            _config = config;
            User = new UserRepository<TUser>(context);
            Role = new RoleRepository<TRole>(context);
        }

        public int Complete()
        {
            return _context.Context.SaveChanges();
        }

        public void ClearRefreshToken()
        {
            var maxDays = _config["refresh_token:max_days"];
            _context.Context.Database.ExecuteSqlRaw($"select prune_refresh_token({maxDays})");
        }
    }
}
