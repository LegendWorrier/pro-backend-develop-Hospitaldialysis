using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Repositories.Interfaces;
using Wasenshi.HemoDialysisPro.Repositories.Repository;
using Wasenshi.HemoDialysisPro.Repository.Core.Interfaces.Base;

namespace Wasenshi.HemoDialysisPro.Repositories.Implementation
{
    public interface IUserRepository : IUserRepository<User>
    {
    }

    public class UserRepository : UserRepository<User>, IUserRepository
    {
        public UserRepository(IContextAdapter context) : base(context)
        {
        }
    }
}
