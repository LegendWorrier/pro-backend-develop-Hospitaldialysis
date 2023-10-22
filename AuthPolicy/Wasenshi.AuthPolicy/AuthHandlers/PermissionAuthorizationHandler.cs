using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wasenshi.AuthPolicy.Requirements;
using Wasenshi.AuthPolicy.Utillities;

namespace Wasenshi.AuthPolicy.AuthHandlers
{
    public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
    {
        public PermissionAuthorizationHandler()
        {
        }
        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
        {
            if (context.User == null)
            {
                return;
            }

            var permission = context.User.Claims.Where(x => x.Type == ClaimsPermissionHelper.PERMISSION_TYPE && x.Value == requirement.Permission);
            if (permission.Any())
            {
                context.Succeed(requirement);
            }
        }
    }
}
