using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Wasenshi.AuthPolicy.Requirements;

namespace Wasenshi.AuthPolicy.AuthHandlers
{
    internal class ResourcePermissionHandlerDefault : AuthorizationHandler<ResourcePermissionRequirement>
    {
        ILogger _logger;
        public ResourcePermissionHandlerDefault(IOptionsMonitor<AuthPolicyOptions> options, ILoggerFactory loggerFactory)
        {
            Options = options.CurrentValue;
            _logger = loggerFactory.CreateLogger<ResourcePermissionHandlerDefault>();
        }

        public AuthPolicyOptions Options { get; }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ResourcePermissionRequirement requirement)
        {
            var pendingReq = context.PendingRequirements;
            if (!context.HasFailed && pendingReq.Any(x => x is ResourcePermissionRequirement))
            {
                switch (Options.MissingHandler)
                {
                    case AuthPolicyOptions.MissingHandlerBehavior.Error:
                        throw new InvalidOperationException($"There is no auth handler setup for '{context.Resource.GetType()}' type. Please add a handler for {context.Resource.GetType()} e.g. a class that inherit and implement {typeof(ResourcePermissionHandler<,>).Name}<{context.Resource.GetType().Name},(UserId Type)>");
                    case AuthPolicyOptions.MissingHandlerBehavior.Warning:
                        Debug.Print($"There is no auth handler setup for '{context.Resource.GetType()}' type, but the type is included in validation process somewhere in the code.");
                        _logger.LogWarning($"There is no auth handler setup for '{context.Resource.GetType()}' type, but the type is included in validation process somewhere in the code.");
                        break;
                    default:
                        break;
                }
            }

            return Task.CompletedTask;
        }
    }
}
