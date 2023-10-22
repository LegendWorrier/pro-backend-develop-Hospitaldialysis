using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Wasenshi.AuthPolicy.Utillities;

namespace Wasenshi.AuthPolicy.Options
{
    /// <summary>
    /// Default implementation where you use ASP.NET's built-in Identity Store for your users.
    /// If you have your own user system, then implement your own <see cref="IUserConfig{TId}"/> class.
    /// </summary>
    /// <typeparam name="TUser"></typeparam>
    public abstract class DefaultUserConfigBase<TUser, TId> : IUserConfig<TId> where TUser : class
    {
        private readonly UserManager<TUser> _userManager;
        private readonly IOptionsMonitor<AuthPolicyOptions> option;

        protected DefaultUserConfigBase(UserManager<TUser> userManager, IOptionsMonitor<AuthPolicyOptions> option)
        {
            _userManager = userManager;
            this.option = option;
        }

        public virtual string[] GetRolesFromClaims(ClaimsPrincipal user)
        {
            return user.Claims.Where(m => m.Type == ClaimTypes.Role).Select(m => m.Value).ToArray();
        }

        public virtual TId GetUserId(ClaimsPrincipal user)
        {
            return Converter.ConvertToId(_userManager.GetUserId(user));
        }

        public virtual async Task<string[]> GetUserRolesAsync(TId userId)
        {
            var userManager = _userManager;
            TUser user = await userManager.FindByIdAsync(Converter.ConvertToString(userId));
            if (user == null)
            {
                return Array.Empty<string>();
            }
            return (await userManager.GetRolesAsync(user)).ToArray();
        }

        public virtual async Task<bool> IsGlobalUser(TId userId)
        {
            var userManager = _userManager;
            TUser user = await userManager.FindByIdAsync(Converter.ConvertToString(userId));
            if (user == null)
            {
                return false;
            }
            var claims = (await userManager.GetClaimsAsync(user)).Select(x => x.Value).Distinct().ToArray();
            return claims.Any(x => x == option.CurrentValue.GlobalPermission);
        }

        public bool IsGlobalUser(ClaimsPrincipal user)
        {
            return user.Claims.Any(x => x.Type == ClaimsPermissionHelper.PERMISSION_TYPE && x.Value == option.CurrentValue.GlobalPermission);
        }

        protected virtual IConverter Converter { get; }

        protected interface IConverter
        {
            Converter<string, TId> ConvertToId { get; }
            Converter<TId, string> ConvertToString { get; }
        }
    }

    /// <summary>
    /// Default implementation where you use ASP.NET's built-in Identity Store for your users.
    /// If you just have different key/model type for user, try inherit and use <see cref="DefaultUserConfigBase{TUser, TId}"/>
    /// But if you have your own user system, then implement your own <see cref="IUserConfig{TId}"/> class.
    /// </summary>
    public class DefaultUserConfig : DefaultUserConfigBase<IdentityUser, string>
    {
        public DefaultUserConfig(UserManager<IdentityUser> userManager, IOptionsMonitor<AuthPolicyOptions> option) : base(userManager, option)
        {
        }
        protected override IConverter Converter => _converter;

        private readonly MyConverter _converter = new();
        private sealed class MyConverter : IConverter
        {
            public Converter<string, string> ConvertToId => (s) => s;

            public Converter<string, string> ConvertToString => (s) => s;
        }
    }
}
