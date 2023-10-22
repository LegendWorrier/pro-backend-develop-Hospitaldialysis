using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using ServiceStack.Redis;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Wasenshi.AuthPolicy;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Services.Interfaces;
using Wasenshi.HemoDialysisPro.Share;
using Wasenshi.HemoDialysisPro.Models.TimezoneUtil;
using Wasenshi.HemoDialysisPro.ViewModels;
using Wasenshi.HemoDialysisPro.Web.Api.Controllers.Utils;

namespace Wasenshi.HemoDialysisPro.Web.Api.Controllers
{
    [Route("api/Patients", Order = -1)]
    [ApiController]
    public class MedicinePrescriptionsController : ControllerBase
    {
        private readonly IPatientService patientService;
        private readonly IVerifyPatientService verifyPatientService;
        private readonly IMedicinePrescriptionService medPrescriptionService;
        private readonly IMasterDataService masterData;
        private readonly IShiftService shiftService;
        private readonly IRedisClient redis;
        private readonly IMapper mapper;
        private readonly IConfiguration configuration;

        public MedicinePrescriptionsController(
            IPatientService patientService,
            IVerifyPatientService verifyPatientService,
            IMedicinePrescriptionService medPrescriptionService,
            IMasterDataService masterData,
            IShiftService shiftService,
            IRedisClient redis,
            IMapper mapper,
            IConfiguration configuration)
        {
            this.patientService = patientService;
            this.verifyPatientService = verifyPatientService;
            this.medPrescriptionService = medPrescriptionService;
            this.masterData = masterData;
            this.shiftService = shiftService;
            this.redis = redis;
            this.mapper = mapper;
            this.configuration = configuration;
        }

        [HttpGet("{patientId}/MedicinePrescriptions")]
        public IActionResult GetMedicinePrescriptionByPatientId(string patientId)
        {
            var result = medPrescriptionService.GetMedicinePrescriptionByPatientId(patientId);

            var data = mapper.Map<IEnumerable<MedicinePrescription>, IEnumerable<MedicinePrescriptionViewModel>>(result);

            return Ok(data);
        }

        [HttpGet("{patientId}/MedicinePrescriptions/Auto")]
        public IActionResult GetMedicinePrescriptionAutoList(string patientId, string timezone)
        {
            TimeZoneInfo tz = null;
            if (timezone != null)
            {
                tz = TimezoneUtils.GetTimeZone(timezone);
            }
            else if (configuration.GetValue<string>("TIMEZONE") != null)
            {
                tz = TimezoneUtils.GetTimeZone(configuration.GetValue<string>("TIMEZONE"));
            }
            var result = medPrescriptionService.GetMedicinePrescriptionAutoList(patientId, tz);

            return Ok(result);
        }

        [HttpGet("MedicinePrescriptions/{id}")]
        public IActionResult GetMedicinePrescription(Guid id)
        {
            var result = medPrescriptionService.GetMedicinePrescription(id);

            return Ok(mapper.Map<MedicinePrescriptionViewModel>(result));
        }

        [Authorize(Roles = Roles.HeadNurseUp)]
        [HttpPost("MedicinePrescriptions")]
        public IActionResult CreatePrescription([FromBody] EditMedicinePrescriptionViewModel prescription)
        {
            MedicinePrescription newPrescription = mapper.Map<MedicinePrescription>(prescription);

            if (!verifyPatientService.VerifyUnit(User, newPrescription.PatientId))
            {
                return Forbid();
            }
            var unitId = patientService.GetPatient(prescription.PatientId).UnitId;
            this.ValidateUnitHeadOrInCharged(unitId, masterData, shiftService, redis);

            medPrescriptionService.CreateMedicinePrescription(newPrescription);

            return Created($"{newPrescription.Id}", mapper.Map<MedicinePrescriptionViewModel>(newPrescription));
        }

        [Authorize(Roles = Roles.HeadNurseUp)]
        [HttpPost("MedicinePrescriptions/{id}")]
        public async Task<IActionResult> EditPrescriptionAsync(Guid id, EditMedicinePrescriptionViewModel prescription)
        {
            MedicinePrescription oldMedPres = medPrescriptionService.GetMedicinePrescription(id);

            if (oldMedPres == null)
            {
                return NotFound("Prescription not found.");
            }

            var unitId = patientService.GetPatient(prescription.PatientId).UnitId;
            this.ValidateUnitHeadOrInCharged(unitId, masterData, shiftService, redis);
            await this.ValidateResourcePermissionAsync(oldMedPres);

            MedicinePrescription editMedPres = mapper.Map(prescription, oldMedPres);

            // Re-validate the edited value
            if (!verifyPatientService.VerifyUnit(User, editMedPres.PatientId))
            {
                return Forbid();
            }

            medPrescriptionService.UpdateMedicinePrescription(editMedPres);

            return Ok();
        }

        [Authorize(Roles = Roles.HeadNurseUp)]
        [HttpDelete("MedicinePrescriptions/{id}")]
        public async Task<IActionResult> DeletePrescriptionAsync(Guid id)
        {
            MedicinePrescription prescription = medPrescriptionService.GetMedicinePrescription(id);

            if (prescription == null)
            {
                return NotFound("Prescription not found.");
            }

            var unitId = patientService.GetPatient(prescription.PatientId).UnitId;
            this.ValidateUnitHeadOrInCharged(unitId, masterData, shiftService, redis);
            await this.ValidateResourcePermissionAsync(prescription);

            medPrescriptionService.DeleteMedicinePrescription(id);

            return Ok();
        }
    }
}
