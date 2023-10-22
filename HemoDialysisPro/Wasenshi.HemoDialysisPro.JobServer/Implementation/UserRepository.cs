using Wasenshi.HemoDialysisPro.Repositories.Interfaces;
using Wasenshi.HemoDialysisPro.Repositories.Repository;
using Wasenshi.HemoDialysisPro.Repository.Core.Interfaces.Base;

namespace Wasenshi.HemoDialysisPro.JobsServer.Implementation
{
    public interface IUserRepository : IUserRepository<UserImp>
    {
    }

    public class UserRepository : UserRepository<UserImp>, IUserRepository
    {
        public UserRepository(IContextAdapter context) : base(context)
        {
        }
    }
}
