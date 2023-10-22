using Microsoft.Extensions.Configuration;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Repositories.Interfaces;
using Wasenshi.HemoDialysisPro.Repository.Core.Interfaces.Base;

namespace Wasenshi.HemoDialysisPro.Repositories.Implementation
{
    public interface IUserUnitOfWork : IUserUnitOfWork<User, Role>
    {
    }

    public class UserUnitOfWork : UserUnitOfWork<User, Role>, IUserUnitOfWork
    {
        public UserUnitOfWork(IContextAdapter context, IConfiguration config) : base(context, config)
        {
        }
    }
}
