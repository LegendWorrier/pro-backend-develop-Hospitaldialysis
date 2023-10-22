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
    public class MedicinePrescriptionHandler : ResourcePermissionHandler<MedicinePrescription, Guid>
    {
        private readonly ILogger<MedicinePrescriptionHandler> logger;

        public MedicinePrescriptionHandler(ILogger<MedicinePrescriptionHandler> logger)
        {
            this.logger = logger;
        }

        private static Patient GetPatient(MedicinePrescription resource, IServiceProvider services)
        {
            var patientService = services.GetService<IPatientService>();
            var patient = patientService.GetPatient(resource.PatientId);
            return patient;
        }

        protected override Guid ResolveOwnerId(MedicinePrescription resource, IServiceProvider services)
        {
            return GetPatient(resource, services)?.DoctorId ?? Guid.Empty;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, ResourcePermissionRequirement requirement, MedicinePrescription resource)
        {
            var patient = GetPatient(resource, requirement.Services);

            var auth = requirement.Services.GetService<IAuthService>();
            if (!auth.VerifyUnit(context.User, new[] { patient.UnitId }))
            {
                logger.LogWarning($"Invalid authorized: required the same unit as patient (unitId: ${patient.UnitId}) [${context.User.GetUsername()}:${context.User.GetUserId()}]");
                context.Fail();
                return;
            }

            // Only validate owner when user is a doctor
            if (!(patient.DoctorId.HasValue && context.User.IsInRole(Roles.Doctor)) || context.User.IsInRole(Roles.Admin))
            {
                context.Succeed(requirement);
                return;
            }

            await base.HandleRequirementAsync(context, requirement, resource);
        }

    }
}
