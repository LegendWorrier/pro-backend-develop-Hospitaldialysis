using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Services.Core.Interfaces;
using Wasenshi.HemoDialysisPro.Services.Interfaces;

namespace Wasenshi.HemoDialysisPro.Services
{
    public class VerifyPatientService : IVerifyPatientService
    {
        private readonly IScheduleService scheduleService;
        private readonly IPatientService patientService;
        private readonly IAuthService authService;
        private readonly IUserInfoService userInfoService;

        public VerifyPatientService(IScheduleService scheduleService, IPatientService patientService, IAuthService authService, IUserInfoService userInfoService)
        {
            this.scheduleService = scheduleService;
            this.patientService = patientService;
            this.authService = authService;
            this.userInfoService = userInfoService;
        }

        // ================================ Verify Units for Patient ==================================
        public bool VerifyUnit(ClaimsPrincipal user, Patient patient)
        {
            bool hasCross = scheduleService.IsCrossScheduleExisted(patient.Id);
            bool verifyUnitPermission = hasCross || authService.VerifyUnit(user, new[] { patient.UnitId });

            bool verifyDoctorUnit;
            if (patient.DoctorId == null || patient.DoctorId == Guid.Empty)
            {
                verifyDoctorUnit = true;
            }
            else // in case doctor id change
            {
                IEnumerable<int> doctorUnits = userInfoService.GetUserUnits(patient.DoctorId.Value);
                verifyDoctorUnit = doctorUnits.Contains(patient.UnitId);
            }

            return verifyUnitPermission && verifyDoctorUnit;
        }

        public bool VerifyUnit(ClaimsPrincipal user, string patientId)
        {
            var patient = patientService.FindPatient(x => x.Id == patientId);
            if (patient == null)
            {
                return false;
            }
            return VerifyUnit(user, patient);
        }
    }
}
