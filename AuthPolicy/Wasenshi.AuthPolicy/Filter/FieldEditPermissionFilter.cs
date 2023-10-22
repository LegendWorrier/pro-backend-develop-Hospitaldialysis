using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Reflection;
using System.Threading.Tasks;
using Wasenshi.AuthPolicy.Attributes;
using Wasenshi.AuthPolicy.Providers;

namespace Wasenshi.AuthPolicy.Filter
{
    internal class FieldEditPermissionFilter : IAsyncActionFilter
    {
        private readonly IAuthorizationService _auth;

        public FieldEditPermissionFilter(IAuthorizationService authorizationService)
        {
            _auth = authorizationService;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            //Safe gaurd check for bypass flag
            var isBypass = (context.ActionDescriptor as ControllerActionDescriptor).MethodInfo.GetCustomAttribute<BypassAuthorizeAttribute>() != null;
            if (!isBypass)
            {
                foreach (var item in context.ActionDescriptor.Parameters)
                {
                    //check for fine-grained level bypassing (per parameter)
                    ParameterInfo paramInfo = ((ControllerParameterDescriptor)item).ParameterInfo;
                    isBypass = paramInfo.GetCustomAttribute<BypassAuthorizeAttribute>() != null;
                    if (isBypass)
                    {
                        continue;
                    }
                    if (item.ParameterType == typeof(string) || item.ParameterType.IsValueType)
                    {
                        continue;
                    }
                    if (!(await _auth.AuthorizeAsync(context.HttpContext.User, context.ActionArguments[item.Name], AuthPolicyProvider.GetPolicyName(AuthPolicyProvider.AuthMode.FieldPermission))).Succeeded)
                    {
                        throw new UnauthorizedException("No permission to edit some protected field.");
                    }
                }
            }

            await next();
        }
    }
}
