using System;
using Wasenshi.HemoDialysisPro.Models.Interfaces;
using Wasenshi.HemoDialysisPro.Repositories.Implementation;
using Wasenshi.HemoDialysisPro.Services.Core.BusinessLogic;

namespace Wasenshi.HemoDialysisPro.Report
{
    public class UserResolver : IUserResolver
    {
        public IUserRepository Repo { get; }

        public UserResolver(IUserRepository userRepo)
        {
            Repo = userRepo;
        }

        public string GetName(Guid userId)
        {
            if (userId == Guid.Empty)
            {
                return null;
            }
            var user = Repo.Get(userId);
            if (user == null)
            {
                return null;
            }
            return Helper.Capitalize(user.GetName());
        }

        public string GetName(IUser user)
        {
            return Helper.Capitalize(user.GetName());
        }

        public string GetEmployeeId(Guid userId)
        {
            if (userId == Guid.Empty)
            {
                return null;
            }
            var user = Repo.Get(userId);
            if (user == null)
            {
                return null;
            }

            return user.EmployeeId;
        }

        public string GetEmployeeId(IUser user)
        {
            return user.EmployeeId;
        }
    }

    public interface IUserResolver
    {
        IUserRepository Repo { get; }
        string GetName(Guid userId);
        string GetName(IUser user);

        string GetEmployeeId(Guid userId);
        string GetEmployeeId(IUser user);
    }
}
