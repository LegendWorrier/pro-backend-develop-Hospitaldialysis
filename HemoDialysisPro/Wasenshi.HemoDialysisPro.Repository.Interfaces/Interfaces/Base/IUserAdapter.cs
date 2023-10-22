using System;
using System.Linq;
using System.Threading.Tasks;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Models.Interfaces;

namespace Wasenshi.HemoDialysisPro.Repository.Core.Interfaces.Base
{
    public interface IUserAdapter
    {
        IQueryable<UserUnit> GetUserUnitMap();
        IQueryable<string> GetUserRoles(Guid userId);
        Task<bool> IsInRoleAsync(IUser user, string role);

        void ClearRefreshToken();
    }
}
