using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Services.Interfaces.Base;

namespace Wasenshi.HemoDialysisPro.Services.Interfaces
{
    public interface IAuthService : IApplicationService
    {
        Task<AuthResponse> GenerateToken(UserResult user, string ipAddress);
        Task<AuthResponse> RefreshToken(string refreshToken, string ipAddress);
        Task<UserResult> AuthenticateAsync(string username, string password);
        Task<IdentityResult> RegisterUserAsync(User user, string password, IList<string> roles);
        Task<IdentityResult> VerifyRoles(IList<string> roles);
        /// <summary>
        /// Check whether current user is eligible for specified list of units.
        /// <br></br>
        /// Empty or null list means bypass the check and will automatically pass the check, unless allowEmptyList is False.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="unitList"></param>
        /// <param name="allowEmptyList">Normally, if no unit list specified, this function should just verify result as valid. Check this to false to inverse that. </param>
        /// <returns></returns>
        bool VerifyUnit(ClaimsPrincipal user, IEnumerable<int> unitList, bool allowEmptyList = true);
        bool RevokeToken(string token, string ipAddress);
        Task<bool> RevokeUserAsync(User user);
    }
}
