using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Wasenshi.AuthPolicy.AuthHandlers;
using Wasenshi.AuthPolicy.Requirements;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Services.Interfaces;
using Wasenshi.HemoDialysisPro.Share;
using Wasenshi.HemoDialysisPro.Utils;

namespace Wasenshi.HemoDialysisPro.Web.Api.AuthPolicy
{
    public class PatientHandler : ResourcePermissionHandler<Patient, Guid>
    {
        private readonly ILogger<PatientHandler> logger;

        public PatientHandler(ILogger<PatientHandler> logger)
        {
            this.logger = logger;
        }

        protected override Guid ResolveOwnerId(Patient resource, IServiceProvider services)
        {
            return resource.DoctorId ?? Guid.Empty;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, ResourcePermissionRequirement requirement, Patient resource)
        {
            var auth = requirement.Services.GetService<IAuthService>();
            if (!auth.VerifyUnit(context.User, new[] { resource.UnitId }))
            {
                logger.LogWarning($"Invalid authorized: required the same unit as patient (unitId: ${resource.UnitId}) [${context.User.GetUsername()}:${context.User.GetUserId()}]");
                context.Fail();
                return;
            }

            // Only validate owner when user is a doctor
            if (!(resource.DoctorId.HasValue && context.User.IsInRole(Roles.Doctor)) || context.User.IsInRole(Roles.Admin))
            {
                context.Succeed(requirement);
                return;
            }

            await base.HandleRequirementAsync(context, requirement, resource);
        }
    }
}
