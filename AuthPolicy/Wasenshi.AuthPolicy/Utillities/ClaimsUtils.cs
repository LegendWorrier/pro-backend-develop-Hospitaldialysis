using System.Security.Claims;
using System.Linq;

namespace Wasenshi.AuthPolicy
{
    public static class ClaimsUtils
    {
        public static bool IsInAnyRole(this ClaimsPrincipal user, string[] roles)
        {
            if (roles == null)
            {
                return false;
            }

            if (roles.Length == 1)
            {
                return roles[0].Split(',').Any(role => user.IsInRole(role.Trim()));
            }
            else
            {
                return roles.Any(x => user.IsInRole(x));
            }
        }
    }
}
