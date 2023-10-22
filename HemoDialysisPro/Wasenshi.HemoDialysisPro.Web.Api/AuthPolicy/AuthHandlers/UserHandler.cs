using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;
using Wasenshi.AuthPolicy.AuthHandlers;
using Wasenshi.AuthPolicy.Requirements;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Services.Interfaces;

namespace Wasenshi.HemoDialysisPro.Web.Api.AuthPolicy
{
    public class UserHandler : ResourcePermissionHandler<User, Guid>
    {
        protected override Guid ResolveOwnerId(User resource, IServiceProvider services)
        {
            return resource.Id;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ResourcePermissionRequirement requirement, User resource)
        {
            var auth = requirement.Services.GetService<IAuthService>();
            // root admin have no units, and we don't want to bypass that, so allowEmptyList must be false.
            if (!auth.VerifyUnit(context.User, resource.Units?.Select(x => x.UnitId), false))
            {
                context.Fail();
                return Task.CompletedTask;
            }

            return base.HandleRequirementAsync(context, requirement, resource);
        }
    }
}
