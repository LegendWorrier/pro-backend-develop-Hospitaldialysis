using Wasenshi.HemoDialysisPro.Models.Interfaces;
using Wasenshi.HemoDialysisPro.Repositories.Interfaces.Base;

namespace Wasenshi.HemoDialysisPro.Repositories.Interfaces
{
    public interface IUserUnitOfWork<TUser, TRole> : IUnitOfWork where TUser : class, IUser where TRole : class, IRole
    {
        IUserRepository<TUser> User { get; }

        IRoleRepository<TRole> Role { get; }

        // ==== stored procedure ============

        void ClearRefreshToken();
    }
}
