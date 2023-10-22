using Wasenshi.HemoDialysisPro.JobsServer.Implementation;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Models.Interfaces;
using Wasenshi.HemoDialysisPro.Repository.Core.Interfaces.Base;

namespace Wasenshi.HemoDialysisPro.JobsServer
{
    public class UserAdapterJobServer : IUserAdapter
    {
        private readonly IUserUnitOfWork uow;

        public UserAdapterJobServer(IUserUnitOfWork uow)
        {
            this.uow = uow;
        }

        public IQueryable<UserUnit> GetUserUnitMap()
        {
            return uow.User.GetUserUnitMap();
        }

        public IQueryable<string> GetUserRoles(Guid userId)
        {
            return uow.Role.GetUserRoles(userId);
        }

        public Task<bool> IsInRoleAsync(IUser user, string role)
        {
            return uow.Role.IsInRoleAsync(user, role);
        }

        public void ClearRefreshToken()
        {
            uow.ClearRefreshToken();
        }
    }
}
