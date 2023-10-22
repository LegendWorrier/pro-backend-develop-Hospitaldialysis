using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.Extensions.Logging;
using ServiceStack.Redis;
using System;
using System.Linq;
using System.Threading.Tasks;
using Wasenshi.HemoDialysisPro.Services.Interfaces;
using Wasenshi.HemoDialysisPro.Share;
using Wasenshi.HemoDialysisPro.Utils;

namespace Wasenshi.HemoDialysisPro.Web.Api.AuthPolicy.AuthHandlers
{
    /// <summary>
    /// Built-in role auth handler already handle role. This one will additionally handle special permission like Unit's Head, or In-Charge Nurse.
    /// (This will be OR logic to original built-in handler.)
    /// </summary>
    public class RoleAndPermissionHandler : AuthorizationHandler<RolesAuthorizationRequirement>
    {
        private readonly IMasterDataService master;
        private readonly IShiftService shift;
        private readonly IRedisClient redis;
        private readonly ILogger<RoleAndPermissionHandler> logger;

        public RoleAndPermissionHandler(IMasterDataService master, IShiftService shift, IRedisClient redis, ILogger<RoleAndPermissionHandler> logger)
        {
            this.master = master;
            this.shift = shift;
            this.redis = redis;
            this.logger = logger;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, RolesAuthorizationRequirement requirement)
        {
            var roles = requirement.AllowedRoles.ToList();
            if (!context.HasSucceeded && roles.Contains(Roles.HeadNurse)) // head nurse is a special case
            {
                var userId = context.User.GetUserIdAsGuid();
                if (userId == Guid.Empty)
                {
                    var error = $"Invalid authorized info: required Head Nurse but no userId (perhaps token is expired?) (${context.User.GetUsername()})";
                    throw new AppException("UNAUTHORIZED", error);
                }
                if (master.IsUnitHead(userId) || shift.hasIncharge(userId, redis.GetAllUnitShifts().Select(x => x.CurrentSection)))
                {
                    context.Succeed(requirement);
                    return Task.CompletedTask;
                }

                // Note: we need to delegate and continue validate on controller's side, because handler cannot know the target unitId.
            }

            return Task.CompletedTask;
        }
    }
}
