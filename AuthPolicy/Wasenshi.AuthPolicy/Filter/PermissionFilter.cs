using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Threading.Tasks;
using Wasenshi.AuthPolicy.Attributes;
using Wasenshi.AuthPolicy.Providers;

namespace Wasenshi.AuthPolicy.Filter
{
    internal class PermissionFilter : IAsyncActionFilter
    {
        private readonly IAuthorizationService _auth;
        private readonly IOptionsMonitor<AuthPolicyOptions> options;

        public PermissionFilter(IAuthorizationService authorizationService, IOptionsMonitor<AuthPolicyOptions> options)
        {
            _auth = authorizationService;
            this.options = options;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var action = (context.ActionDescriptor as ControllerActionDescriptor);
            //Safe gaurd check for bypass flag
            var isBypass = action.MethodInfo.GetCustomAttribute<BypassAuthorizeAttribute>() != null;
            if (!isBypass)
            {
                var controllerPermissions = action.ControllerTypeInfo.GetCustomAttributes<PermissionAuthorizeAttribute>();
                var methodPermissions = action.MethodInfo.GetCustomAttributes<PermissionAuthorizeAttribute>();
                var hasPriorityPermission = false;
                foreach (var permission in controllerPermissions.Where(x => x.Priority))
                {
                    if (await CheckPermission(permission, context))
                    {
                        hasPriorityPermission = true;
                    }
                }
                // special bypassing permission for this action group
                if (controllerPermissions.Any(x => x.Priority) && hasPriorityPermission)
                {
                    await next();
                    return;
                }

                // global permission for rootadmin (always bypass all other)
                if (!string.IsNullOrWhiteSpace(options.CurrentValue.GlobalPermission) &&
                    (await _auth.AuthorizeAsync(context.HttpContext.User, null, AuthPolicyProvider.GetPolicyName(AuthPolicyProvider.AuthMode.Permission, options.CurrentValue.GlobalPermission))).Succeeded)
                {
                    await next();
                    return;
                }

                // Required permission for this action
                foreach (var permission in methodPermissions.Where(x => x.Priority))
                {
                    if (!await CheckPermission(permission, context))
                    {
                        throw new UnauthorizedException("No permission for target action.");
                    }
                }
                if (methodPermissions.Any(x => x.Priority))
                {
                    await next();
                    return;
                }

                // -------------- Check for any permission and validate -----------------

                bool hasControllerPermission = true;
                foreach (var permission in controllerPermissions.Where(x => !x.Priority))
                {
                    if (!await CheckPermission(permission, context))
                    {
                        hasControllerPermission = false;
                    }
                }
                if (controllerPermissions.Any() && hasControllerPermission)
                {
                    await next();
                    return;
                }

                bool hasMethodPermission = true;
                foreach (var permission in methodPermissions.Where(x => !x.Priority))
                {
                    if (!await CheckPermission(permission, context))
                    {
                        hasMethodPermission = false;
                    }
                }

                if (methodPermissions.Any(x => !x.Priority) && hasMethodPermission)
                {
                    await next();
                    return;
                }

                if (!hasControllerPermission || !hasMethodPermission)
                {
                    throw new UnauthorizedException("No permission for target action.");
                }
            }

            await next();
        }

        private async Task<bool> CheckPermission(PermissionAuthorizeAttribute permission, ActionExecutingContext context)
        {
            bool hasPermission = false;
            foreach (var check in permission.PermissionName.Split(',').Select(x => x.Trim()))
            {
                if ((await _auth.AuthorizeAsync(context.HttpContext.User, null, AuthPolicyProvider.GetPolicyName(AuthPolicyProvider.AuthMode.Permission, check))).Succeeded)
                {
                    hasPermission = true;
                    break;
                }
            }
            return hasPermission;
        }
    }
}
