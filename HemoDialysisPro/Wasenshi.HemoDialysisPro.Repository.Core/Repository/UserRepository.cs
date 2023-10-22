using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Models.Interfaces;
using Wasenshi.HemoDialysisPro.Repositories.Interfaces;
using Wasenshi.HemoDialysisPro.Repository.Core.Interfaces.Base;

namespace Wasenshi.HemoDialysisPro.Repositories.Repository
{
    public class UserRepository<TUser> : Repository<TUser, Guid>, IUserRepository<TUser> where TUser : class, IUser
    {
        public UserRepository(IContextAdapter context) : base(context)
        {
        }

        public IQueryable<UserUnit> GetUserUnitMap()
        {
            return context.UserUnits.AsNoTracking();
        }

        protected override IQueryable<TUser> GetQueryWithIncludes()
        {
            return base.GetQueryWithIncludes().Include(x => x.Units).AsSplitQuery();
        }
    }
}
