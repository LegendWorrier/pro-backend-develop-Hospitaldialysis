using Microsoft.AspNetCore.Authorization;
using System;
using System.Threading.Tasks;
using Wasenshi.AuthPolicy.Options;
using Wasenshi.AuthPolicy.Requirements;

namespace Wasenshi.AuthPolicy.AuthHandlers
{
    public abstract class ResourcePermissionHandler<TResource, TId> : AuthorizationHandler<ResourcePermissionRequirement, TResource> where TResource : class
    {
        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, ResourcePermissionRequirement requirement, TResource resource)
        {
            IUserConfig<TId> userConfig = requirement.Options.GetUserConfig<IUserConfig<TId>, TId>(requirement.Services);
            IAuthPolicy<TId> authPolicy = requirement.Options.GetAuthPolicy<TId>(requirement.PermissionPolicyName);

            TId ownerId = ResolveOwnerId(resource, requirement.Services);
            string[] ownerRoles = await userConfig.GetUserRolesAsync(ownerId);

            TId userId = userConfig.GetUserId(context.User);
            string[] userRoles = userConfig.GetRolesFromClaims(context.User);

            bool userGlobal = userConfig.IsGlobalUser(context.User);
            bool ownerGlobal = await userConfig.IsGlobalUser(ownerId);

            var validateContext = new ValidateContext<TId>(userId, ownerId, userRoles, ownerRoles, userGlobal, ownerGlobal);

            if (authPolicy.Validate(validateContext))
            {
                context.Succeed(requirement);
            }
            else
            {
                context.Fail();
            }
        }

        protected abstract TId ResolveOwnerId(TResource resource, IServiceProvider services);
    }
}
