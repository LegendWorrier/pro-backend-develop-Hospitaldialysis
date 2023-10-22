using System;
using System.Linq;
using System.Threading.Tasks;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Models.Interfaces;
using Wasenshi.HemoDialysisPro.Repositories.Implementation;
using Wasenshi.HemoDialysisPro.Repository.Core.Interfaces.Base;

namespace Wasenshi.HemoDialysisPro.Repositories
{
    public class UserAdapterAspNet : IUserAdapter
    {
        private readonly IUserUnitOfWork userUow;

        public UserAdapterAspNet(IUserUnitOfWork userUow)
        {
            this.userUow = userUow;
        }

        public IQueryable<UserUnit> GetUserUnitMap()
        {
            return userUow.User.GetUserUnitMap();
        }

        public IQueryable<string> GetUserRoles(Guid userId)
        {
            return userUow.Role.GetUserRoles(userId);
        }

        public Task<bool> IsInRoleAsync(IUser user, string role)
        {
            return userUow.Role.IsInRoleAsync(user, role);
        }

        public void ClearRefreshToken()
        {
            userUow.ClearRefreshToken();
        }
    }
}
