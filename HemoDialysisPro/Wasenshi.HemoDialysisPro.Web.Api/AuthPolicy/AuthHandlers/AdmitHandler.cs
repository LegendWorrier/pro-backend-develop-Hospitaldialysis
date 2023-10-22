using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using Wasenshi.AuthPolicy.AuthHandlers;
using Wasenshi.AuthPolicy.Requirements;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Services.Interfaces;

namespace Wasenshi.HemoDialysisPro.Web.Api.AuthPolicy
{
    public class AdmitHandler : ResourcePermissionHandler<Admission, Guid>
    {
        protected override Guid ResolveOwnerId(Admission resource, IServiceProvider services)
        {
            return Guid.Empty;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, ResourcePermissionRequirement requirement, Admission resource)
        {
            var auth = requirement.Services.GetService<IAuthService>();
            var patientService = requirement.Services.GetService<IPatientService>();
            var patient = patientService.GetPatient(resource.PatientId);
            if (!auth.VerifyUnit(context.User, new[] { patient.UnitId }))
            {
                context.Fail();
                return;
            }

            await base.HandleRequirementAsync(context, requirement, resource);
        }
    }
}
