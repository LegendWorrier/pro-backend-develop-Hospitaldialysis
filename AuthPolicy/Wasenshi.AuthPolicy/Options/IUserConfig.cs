using System.Security.Claims;
using System.Threading.Tasks;

namespace Wasenshi.AuthPolicy
{
    public interface IUserConfig<TId>
    {
        /// <summary>
        /// Get current user id from the token's claims
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        TId GetUserId(ClaimsPrincipal user);
        /// <summary>
        /// Get current user roles from the token's claims
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        string[] GetRolesFromClaims(ClaimsPrincipal user);
        /// <summary>
        /// Get roles of any specific user by id
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<string[]> GetUserRolesAsync(TId userId);

        Task<bool> IsGlobalUser(TId userId);
        bool IsGlobalUser(ClaimsPrincipal user);
    }
}
