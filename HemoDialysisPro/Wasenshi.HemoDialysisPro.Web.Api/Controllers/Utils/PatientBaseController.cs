using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Services.Interfaces;
using Wasenshi.HemoDialysisPro.Share;
using Wasenshi.HemoDialysisPro.Utils;

namespace Wasenshi.HemoDialysisPro.Web.Api.Controllers
{
    public class PatientBaseController : ControllerBase
    {
        protected readonly IPatientService PatientService;
        protected readonly IVerifyPatientService VerifyPatienService;
        protected readonly IScheduleService ScheduleService;

        public PatientBaseController(IPatientService patientService, IVerifyPatientService verifyPatientService, IScheduleService scheduleService)
        {
            this.PatientService = patientService;
            this.VerifyPatienService = verifyPatientService;
            this.ScheduleService = scheduleService;
        }

        // =============== Utils ====================

        protected Patient FindPatient(string patientId)
        {
            if (string.IsNullOrWhiteSpace(patientId))
            {
                return null;
            }

            bool hasCross = ScheduleService.IsCrossScheduleExisted(patientId);
            List<int> units = User.GetUnitList().ToList();
            var whereCondition = User.IsInRole(Roles.PowerAdmin) || hasCross ?
                (Expression<Func<Patient, bool>>)null
                : x => units.Contains(x.UnitId);
            Expression<Func<Patient, bool>> filterByPatientId = x => x.Id == patientId;
            return PatientService.FindPatient(filterByPatientId.AndAlso(whereCondition));
        }
    }
}
