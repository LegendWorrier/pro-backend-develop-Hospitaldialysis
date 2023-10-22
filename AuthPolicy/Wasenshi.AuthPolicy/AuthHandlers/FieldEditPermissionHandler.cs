using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using System.Collections;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;
using Wasenshi.AuthPolicy.Attributes;
using Wasenshi.AuthPolicy.Providers;
using Wasenshi.AuthPolicy.Requirements;
using Wasenshi.AuthPolicy.Utillities;

namespace Wasenshi.AuthPolicy.AuthHandlers
{
    internal class FieldEditPermissionHandler : AuthorizationHandler<FieldEditPermissionRequirement>
    {
        private readonly IOptionsMonitor<AuthPolicyOptions> option;

        public FieldEditPermissionHandler(IOptionsMonitor<AuthPolicyOptions> option)
        {
            this.option = option;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, FieldEditPermissionRequirement requirement)
        {
            if (CheckRestrictOrForbid(context.User, context.Resource))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }

        internal bool CheckRestrictOrForbid(ClaimsPrincipal user, object data)
        {
            if (data == null)
            {
                return true;
            }

            bool isGlobalUser = user.Claims.Any(x => x.Type == ClaimsPermissionHelper.PERMISSION_TYPE && x.Value == option.CurrentValue.GlobalPermission);
            if (isGlobalUser)
            {
                return true;
            }

            foreach (PropertyInfo prop in data.GetType().GetProperties())
            {
                var roleRestrict = prop.GetCustomAttribute<RoleRestrictAttribute>();
                var forbid = prop.GetCustomAttribute<RoleForbidAttribute>();
                var permissionRestricts = prop.GetCustomAttributes<PermissionRestrictAttribute>();

                var typeInfo = prop.PropertyType;
                var value = prop.GetValue(data);

                // Actual Checking
                if (typeInfo == typeof(string) || typeInfo.IsValueType)
                {
                    if (value != null)
                    {
                        if (roleRestrict != null && !(user?.IsInAnyRole(roleRestrict.Role) ?? false))
                        {
                            return false;
                        }
                        else if (forbid != null && (((user?.IsInAnyRole(forbid.Role) ?? false) && user.FindAll(ClaimTypes.Role).Count() == 1) || forbid.Role.Length == 0 && (user?.Identity == null || user.FindFirst(ClaimTypes.Role) == null)))
                        {
                            return false;
                        }
                        else
                        {
                            foreach (var item in permissionRestricts)
                            {
                                if (!CheckPermission(item, user))
                                {
                                    return false;
                                }
                            }
                        }
                    }
                }
                else if (value is IEnumerable || typeof(IEnumerable).IsAssignableFrom(typeInfo))
                {
                    if (value == null)
                    {
                        continue;
                    }
                    foreach (object item in value as IEnumerable)
                    {
                        if (!CheckRestrictOrForbid(user, item))
                        {
                            return false;
                        }
                    }
                }
                else if (typeInfo.IsClass && !CheckRestrictOrForbid(user, value))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool CheckPermission(PermissionRestrictAttribute permission, ClaimsPrincipal user)
        {
            return permission.Permissions.Any(permission => user.Claims.Any(x => x.Type == ClaimsPermissionHelper.PERMISSION_TYPE && x.Value == permission));
        }
        
    }
}
