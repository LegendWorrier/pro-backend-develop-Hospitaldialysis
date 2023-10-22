using System.Linq;
using Wasenshi.HemoDialysisPro.Models.Interfaces;
using Wasenshi.HemoDialysisPro.Repositories;

namespace Wasenshi.HemoDialysisPro.Repository.Core.Interfaces.Base
{
    public interface IContextAdapter
    {
        IQueryable<IUser> GetUsers();
        IQueryable<IRole> GetRoles();
        IQueryable<IUserRole> GetUserRoles();


        IApplicationDbContext Context { get; }
    }
}
