using System;
using System.Linq;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Models.Interfaces;
using Wasenshi.HemoDialysisPro.Repositories.Interfaces.Base;

namespace Wasenshi.HemoDialysisPro.Repositories.Interfaces
{
    public interface IUserRepository<TUser> : IRepository<TUser, Guid> where TUser : class, IUser
    {
        IQueryable<UserUnit> GetUserUnitMap();
    }
}
