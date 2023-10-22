using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Models.Infrastructor;

namespace Wasenshi.HemoDialysisPro.Repositories
{
    public class UserLoginStore : IUserLoginStore<User>
    {
        private readonly ApplicationDbContext context;
        private readonly UserManager<User> userManager;

        public UserLoginStore(ApplicationDbContext context, UserManager<User> userManager)
        {
            this.context = context;
            this.userManager = userManager;
        }

        public async Task AddLoginAsync(User user, UserLoginInfo login, CancellationToken cancellationToken)
        {
            await context.UserLogins.AddAsync(new UserLogin
            {
                LoginProvider = login.LoginProvider,
                ProviderDisplayName = login.ProviderDisplayName,
                ProviderKey = login.ProviderKey,
                UserId = user.Id
            }, cancellationToken);

            await context.SaveChangesAsync(cancellationToken);
        }

        public Task<IdentityResult> CreateAsync(User user, CancellationToken cancellationToken)
        {
            return userManager.CreateAsync(user);
        }

        public Task<IdentityResult> DeleteAsync(User user, CancellationToken cancellationToken)
        {
            return userManager.DeleteAsync(user);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                userManager.Dispose();
            }
        }

        ~UserLoginStore() { Dispose(false); }

        public Task<User> FindByIdAsync(string userId, CancellationToken cancellationToken)
        {
            return userManager.FindByIdAsync(userId);
        }

        public Task<User> FindByLoginAsync(string loginProvider, string providerKey, CancellationToken cancellationToken)
        {
            var login = context.UserLogins.FirstOrDefault(x => x.LoginProvider == loginProvider && x.ProviderKey == providerKey);
            if (login == null)
            {
                return Task.FromResult<User>(null);
            }
            return userManager.FindByIdAsync(login.UserId.ToString());
        }

        public Task<User> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<IList<UserLoginInfo>> GetLoginsAsync(User user, CancellationToken cancellationToken)
        {
            var loginList = context.UserLogins.Where(x => x.UserId == user.Id).ToList();
            var result = loginList.Select(x => new UserLoginInfo(x.LoginProvider, x.ProviderKey, x.ProviderDisplayName)).ToList();
            return Task.FromResult((IList<UserLoginInfo>)result);
        }

        public Task<string> GetNormalizedUserNameAsync(User user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetUserIdAsync(User user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetUserNameAsync(User user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task RemoveLoginAsync(User user, string loginProvider, string providerKey, CancellationToken cancellationToken)
        {
            var login = context.UserLogins.FirstOrDefault(x => x.LoginProvider == loginProvider && x.ProviderKey == providerKey && x.UserId == user.Id);
            if (login == null)
            {
                return Task.CompletedTask;
            }
            context.UserLogins.Remove(login);
            return context.SaveChangesAsync(cancellationToken);
        }

        public Task SetNormalizedUserNameAsync(User user, string normalizedName, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task SetUserNameAsync(User user, string userName, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<IdentityResult> UpdateAsync(User user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
