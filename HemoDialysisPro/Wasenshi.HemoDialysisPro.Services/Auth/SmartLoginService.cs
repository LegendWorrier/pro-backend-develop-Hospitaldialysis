using Microsoft.AspNetCore.Identity;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Repositories;
using Wasenshi.HemoDialysisPro.Repositories.Implementation;
using Wasenshi.HemoDialysisPro.Services.Interfaces;
using Wasenshi.HemoDialysisPro.Utils;
using Wasenshi.HemoDialysisPro.Services.SmartLogin;

namespace Wasenshi.HemoDialysisPro.Services.Auth
{
    public class SmartLoginService : ISmartLoginService
    {
        private readonly UserManager<User> userManager;
        private readonly IUserUnitOfWork userUnit;
        private readonly UserLoginStore loginStore;
        private readonly OneTimeTokenProvider ott;

        public SmartLoginService(UserManager<User> userManager, IUserUnitOfWork userUnit, UserLoginStore loginStore, OneTimeTokenProvider ott)
        {
            this.userManager = userManager;
            this.userUnit = userUnit;
            this.loginStore = loginStore;
            this.ott = ott;
        }

        public async Task<string> GenerateOneTimeToken(Guid userId)
        {
            User user = await userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                throw new AppException("User not found.");
            }
            var token = await userManager.GenerateUserTokenAsync(user, OneTimeToken.ID, OneTimeToken.ID);
            await userManager.AddLoginAsync(user, new UserLoginInfo(OneTimeToken.ID, token, OneTimeToken.ID));

            return token;
        }

        public async Task<UserResult> LoginWithOneTimeToken(string token)
        {
            User user = await userManager.FindByLoginAsync(OneTimeToken.ID, token);
            if (user != null)
            {
                bool valid = await ott.ValidateAsync(OneTimeToken.ID, token, userManager, user); // validate first

                await userManager.RemoveLoginAsync(user, OneTimeToken.ID, token); // then ensure it can only be used once.

                if (!valid)
                {
                    return null;
                }

                var roles = await userManager.GetRolesAsync(user);
                var units = userUnit.User.GetUserUnitMap().Where(x => x.UserId == user.Id).ToList();
                user.Units = units;
                return new UserResult
                {
                    User = user,
                    Roles = roles
                };
            }
            return null;
        }

        public async Task<string> GenerateTokenForUser(Guid userId, string password)
        {
            User user = await userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                throw new AppException("User not found.");
            }
            if (!await userManager.CheckPasswordAsync(user, password))
            {
                // unauthorized
                return null;
            }

            await RevokeAllTokenForUser(userId);

            var token = await userManager.GenerateUserTokenAsync(user, SmartLogin.SmartLogin.ID, SmartLogin.SmartLogin.ID);
            await userManager.AddLoginAsync(user, new UserLoginInfo(SmartLogin.SmartLogin.ID, token, SmartLogin.SmartLogin.ID));

            return token;
        }

        public async Task<UserResult> LoginWithToken(string token)
        {
            User user = await userManager.FindByLoginAsync(SmartLogin.SmartLogin.ID, token);
            if (user != null)
            {
                var roles = await userManager.GetRolesAsync(user);
                var units = userUnit.User.GetUserUnitMap().Where(x => x.UserId == user.Id).ToList();
                user.Units = units;
                return new UserResult
                {
                    User = user,
                    Roles = roles
                };
            }
            return null;
        }

        public async Task<bool> RevokeAllTokenForUser(Guid userId)
        {
            User user = await userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                throw new AppException("User not found.");
            }
            var currentTokens = await loginStore.GetLoginsAsync(user, CancellationToken.None);
            if (currentTokens.Count == 0)
            {
                return false;
            }

            foreach (var token in currentTokens)
            {
                await userManager.RemoveLoginAsync(user, token.LoginProvider, token.ProviderKey);
            }

            return true;
        }
    }
}
