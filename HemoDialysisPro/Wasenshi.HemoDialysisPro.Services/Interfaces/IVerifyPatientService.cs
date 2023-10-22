using System.Security.Claims;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Services.Interfaces.Base;

namespace Wasenshi.HemoDialysisPro.Services.Interfaces
{
    public interface IVerifyPatientService : IApplicationService
    {
        bool VerifyUnit(ClaimsPrincipal user, Patient patient);
        bool VerifyUnit(ClaimsPrincipal user, string patientId);
    }
}
