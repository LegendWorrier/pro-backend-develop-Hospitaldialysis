using Microsoft.AspNetCore.Identity;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Wasenshi.AuthPolicy.Utillities
{
    public static class ClaimsPermissionHelper
    {
        public const string PERMISSION_TYPE = "Permission";

        public static async Task AddPermissionToRole<TRole>(this RoleManager<TRole> roleManager, string roleName, params string[] permissions)
            where TRole : class
        {
            var role = await roleManager.FindByNameAsync(roleName);
            await roleManager.AddPermissionToRole(role, permissions);
        }
        public static async Task AddPermissionToRole<TRole>(this RoleManager<TRole> roleManager, TRole role, params string[] permissions)
            where TRole : class
        {
            var allClaims = await roleManager.GetClaimsAsync(role);
            foreach (var item in permissions.SelectMany(x => x.Split(",").Select(x => x.Trim()))
                .Where(item => !allClaims.Any(a => a.Type == PERMISSION_TYPE && a.Value == item)))
            {
                await roleManager.AddClaimAsync(role, new Claim(PERMISSION_TYPE, item));
            }
        }

        public static async Task RemovePermissionFromRole<TRole>(this RoleManager<TRole> roleManager, string roleName, params string[] permissions)
            where TRole : class
        {
            var role = await roleManager.FindByNameAsync(roleName);
            await roleManager.RemovePermissionFromRole(role, permissions);
        }

        public static async Task RemovePermissionFromRole<TRole>(this RoleManager<TRole> roleManager, TRole role, params string[] permissions)
            where TRole : class
        {
            var allClaims = await roleManager.GetClaimsAsync(role);
            foreach (var item in permissions.SelectMany(x => x.Split(",").Select(x => x.Trim()))
                .Where(item => allClaims.Any(a => a.Type == PERMISSION_TYPE && a.Value == item)))
            {
                await roleManager.RemoveClaimAsync(role, new Claim(PERMISSION_TYPE, item));
            }
        }



        public static async Task AddPermissionToUser<TUser>(this UserManager<TUser> userManager, string username, params string[] permissions)
            where TUser : class
        {
            var user = await userManager.FindByNameAsync(username);
            await userManager.AddPermissionToUser(user, permissions);
        }
        public static async Task AddPermissionToUser<TUser>(this UserManager<TUser> userManager, TUser user, params string[] permissions)
            where TUser : class
        {
            var allClaims = await userManager.GetClaimsAsync(user);
            foreach (var item in permissions.SelectMany(x => x.Split(",").Select(x => x.Trim()))
                .Where(item => !allClaims.Any(a => a.Type == PERMISSION_TYPE && a.Value == item)))
            {
                await userManager.AddClaimAsync(user, new Claim(PERMISSION_TYPE, item));
            }
        }

        public static async Task RemovePermissionFromUser<TUser>(this UserManager<TUser> userManager, string username, params string[] permissions)
            where TUser : class
        {
            var user = await userManager.FindByNameAsync(username);
            await userManager.RemovePermissionFromUser(user, permissions);
        }

        public static async Task RemovePermissionFromUser<TUser>(this UserManager<TUser> userManager, TUser user, params string[] permissions)
            where TUser : class
        {
            var allClaims = await userManager.GetClaimsAsync(user);
            foreach (var item in permissions.SelectMany(x => x.Split(",").Select(x => x.Trim()))
                .Where(item => allClaims.Any(a => a.Type == PERMISSION_TYPE && a.Value == item)))
            {
                await userManager.RemoveClaimAsync(user, new Claim(PERMISSION_TYPE, item));
            }
        }
    }
}
