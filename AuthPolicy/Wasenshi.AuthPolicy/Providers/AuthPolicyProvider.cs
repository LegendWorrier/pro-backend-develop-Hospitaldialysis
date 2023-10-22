using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using Wasenshi.AuthPolicy.Requirements;

namespace Wasenshi.AuthPolicy.Providers
{
    internal class AuthPolicyProvider : IAuthorizationPolicyProvider
    {
        public const string AUTH_PREFIX = "AUTH";
        public const string PERMISSION = "CHECKPERMISSION";
        public const string RESOURCE = "CHECKRESOURCE";
        public const string FIELD = "CHECKFIELD";

        private readonly IOptionsMonitor<AuthPolicyOptions> _options;
        private readonly IHttpContextAccessor _contextAccessor;

        private DefaultAuthorizationPolicyProvider BackupPolicyProvider { get; }

        public AuthPolicyProvider(IOptions<AuthorizationOptions> authOption, IOptionsMonitor<AuthPolicyOptions> options, IHttpContextAccessor contextAccessor)
        {
            BackupPolicyProvider = new DefaultAuthorizationPolicyProvider(authOption);
            _options = options;
            _contextAccessor = contextAccessor;
        }

        public Task<AuthorizationPolicy> GetDefaultPolicyAsync()
        {
            return BackupPolicyProvider.GetDefaultPolicyAsync();
        }

        public Task<AuthorizationPolicy> GetFallbackPolicyAsync()
        {
            return BackupPolicyProvider.GetFallbackPolicyAsync();
        }

        public Task<AuthorizationPolicy> GetPolicyAsync(string policyName)
        {
            if (policyName.StartsWith(AUTH_PREFIX))
            {
                var policy = policyName.Substring(5).Split('_');
                var builder = new AuthorizationPolicyBuilder();
                switch (policy[0])
                {
                    case PERMISSION:
                        return Task.FromResult(builder.AddRequirements(new PermissionRequirement(policy[1])).Build());
                    case RESOURCE:
                        var requirement = new ResourcePermissionRequirement
                        {
                            PermissionPolicyName = policy[1],
                            Options = _options.CurrentValue,
                            Services = _contextAccessor.HttpContext.RequestServices
                        };
                        return Task.FromResult(builder.AddRequirements(requirement).Build());
                    case FIELD:
                        return Task.FromResult(builder.AddRequirements(new FieldEditPermissionRequirement()).Build());
                    default:
                        break;
                }
            }

            return BackupPolicyProvider.GetPolicyAsync(policyName);
        }
        public enum AuthMode
        {
            Permission,
            ResourcePermission,
            FieldPermission
        }
        public static string GetPolicyName(AuthMode authMode, string policyName = null)
        {
            string mode = "";
            switch (authMode)
            {
                case AuthMode.Permission:
                    mode = PERMISSION;
                    break;
                case AuthMode.ResourcePermission:
                    mode = RESOURCE;
                    break;
                case AuthMode.FieldPermission:
                    mode = FIELD;
                    break;
                default:
                    break;
            }
            return $"{AUTH_PREFIX}_{mode}_{policyName}";
        }
    }
}
