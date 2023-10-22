using Microsoft.Extensions.Configuration;
using Wasenshi.HemoDialysisPro.Repositories;
using Wasenshi.HemoDialysisPro.Repositories.Interfaces;
using Wasenshi.HemoDialysisPro.Repository.Core.Interfaces.Base;

namespace Wasenshi.HemoDialysisPro.JobsServer.Implementation
{
    public interface IUserUnitOfWork : IUserUnitOfWork<UserImp, RoleImp>
    {
    }

    public class UserUnitOfWork : UserUnitOfWork<UserImp, RoleImp>, IUserUnitOfWork
    {
        public UserUnitOfWork(IContextAdapter context, IConfiguration config) : base(context, config)
        {
        }
    }
}
