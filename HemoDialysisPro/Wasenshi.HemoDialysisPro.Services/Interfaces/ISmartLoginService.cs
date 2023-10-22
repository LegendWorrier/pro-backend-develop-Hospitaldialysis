using System;
using System.Threading.Tasks;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Services.Interfaces.Base;

namespace Wasenshi.HemoDialysisPro.Services.Interfaces
{
    public interface ISmartLoginService : IApplicationService
    {
        /// <summary>
        /// Generate new One-Time-Token used for integrated app.
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<string> GenerateOneTimeToken(Guid userId);

        /// <summary>
        /// Login a user with One-Time-Token. Logging in with this token will immediately invalidate the token. (It can only be used once.)
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<UserResult> LoginWithOneTimeToken(string token);

        /// <summary>
        /// Generate new Smart Login Token for a user (user must provide his/her current password). Return null if the password is wrong.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        Task<string> GenerateTokenForUser(Guid userId, string password);

        /// <summary>
        /// Login a user with smart login token. Return a user if success, othwise, return null.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<UserResult> LoginWithToken(string token);

        /// <summary>
        /// Manually revoke all current token(s) for a user.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<bool> RevokeAllTokenForUser(Guid userId);
    }
}
