using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Wasenshi.AuthPolicy;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Services.Interfaces;
using Wasenshi.HemoDialysisPro.ViewModels;

namespace Wasenshi.HemoDialysisPro.Web.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AvShuntsController : PatientBaseController
    {
        private readonly IAvShuntService avShuntService;
        private readonly IMapper mapper;

        public AvShuntsController(
            IAvShuntService avShuntService,
            IPatientService patientService,
            IVerifyPatientService verifyPatientService,
            IScheduleService scheduleService,
            IMapper mapper) : base(patientService, verifyPatientService, scheduleService)
        {
            this.avShuntService = avShuntService;
            this.mapper = mapper;
        }


        [HttpGet("view/{patientId}")]
        public IActionResult GetAvShuntView(string patientId)
        {
            if (FindPatient(patientId) == null)
            {
                return NotFound();
            }

            AVResult result = avShuntService.GetAvViewResultByPatientId(patientId);
            return Ok(new
            {
                AvShunts = mapper.Map<IEnumerable<AVShuntViewModel>>(result.AvShunts),
                IssueTreatments = mapper.Map<IEnumerable<AVShuntIssueTreatmentViewModel>>(result.AvShuntIssueTreatments)
            });
        }

        [HttpGet("list/{patientId}")]
        public IActionResult GetAvShuntList(string patientId)
        {
            if (FindPatient(patientId) == null)
            {
                return NotFound();
            }

            IEnumerable<AVShunt> result = avShuntService.GetAvListByPatientId(patientId);
            return Ok(mapper.Map<IEnumerable<AVShuntViewModel>>(result));
        }

        [HttpPost("new/{patientId}")]
        public IActionResult CreateAvShunt(string patientId, [FromBody] EditAVShuntViewModel avShunt)
        {
            avShunt.PatientId = patientId;
            AVShunt newAvShunt = mapper.Map<AVShunt>(avShunt);

            if (!VerifyPatienService.VerifyUnit(User, avShunt.PatientId))
            {
                return Forbid();
            }

            avShuntService.CreateAvShunt(newAvShunt);

            return Created($"{newAvShunt.Id}", mapper.Map<AVShuntViewModel>(newAvShunt));
        }

        [HttpGet("{id}")]
        public IActionResult GetAvShunt(Guid id)
        {
            AVShunt result = avShuntService.GetAvShunt(id);

            if (!VerifyPatienService.VerifyUnit(User, result.PatientId))
            {
                return Forbid();
            }

            return Ok(mapper.Map<IEnumerable<AVShuntViewModel>>(result));
        }

        [HttpPost("{id}")]
        public async Task<IActionResult> EditAvShuntAsync(Guid id, EditAVShuntViewModel avShunt)
        {
            // Verify Patient
            Patient patient = FindPatient(avShunt.PatientId);
            if (patient == null)
            {
                return Forbid();
            }
            await this.ValidateResourcePermissionAsync(patient);

            avShunt.Id = id;
            AVShunt editAvShunt = mapper.Map<AVShunt>(avShunt);

            bool result = avShuntService.EditAvShunt(editAvShunt);

            if (!result)
            {
                return NotFound();
            }

            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAvShuntAsync(Guid id)
        {
            AVShunt avShunt = avShuntService.GetAvShunt(id);
            if (avShunt == null)
            {
                return NotFound("AVShunt not found.");
            }

            Patient patient = PatientService.GetPatient(avShunt.PatientId);

            if (patient == null)
            {
                return NotFound("Patient not found.");
            }

            await this.ValidateResourcePermissionAsync(patient);

            avShuntService.DeleteAvShunt(id);

            return Ok();
        }

        [HttpPost("issues/new/{patientId}")]
        public IActionResult CreateAvShuntIssue(string patientId, [FromBody] AVShuntIssueTreatmentViewModel issueTreatment)
        {
            issueTreatment.PatientId = patientId;
            AVShuntIssueTreatment newIssueTreatment = mapper.Map<AVShuntIssueTreatment>(issueTreatment);

            if (!VerifyPatienService.VerifyUnit(User, issueTreatment.PatientId))
            {
                return Forbid();
            }

            avShuntService.CreateIssueTreatment(newIssueTreatment);

            return Created($"{newIssueTreatment.Id}", mapper.Map<AVShuntIssueTreatmentViewModel>(newIssueTreatment));
        }

        [HttpPost("issues/{id}")]
        public async Task<IActionResult> EditAvShuntIssueAsync(Guid id, AVShuntIssueTreatmentViewModel issueTreatment)
        {
            // Verify Patient
            Patient patient = FindPatient(issueTreatment.PatientId);
            if (patient == null)
            {
                return Forbid();
            }
            await this.ValidateResourcePermissionAsync(patient);

            issueTreatment.Id = id;
            AVShuntIssueTreatment editIssueTreatment = mapper.Map<AVShuntIssueTreatment>(issueTreatment);

            bool result = avShuntService.EditIssueTreatment(editIssueTreatment);

            if (!result)
            {
                return NotFound();
            }

            return Ok();
        }

        [HttpDelete("issues/{id}")]
        public async Task<IActionResult> DeleteAvShuntIssueAsync(Guid id)
        {
            AVShuntIssueTreatment issueTreatment = avShuntService.GetIssueTreatment(id);
            if (issueTreatment == null)
            {
                return NotFound("AVShunt Issue not found.");
            }

            Patient patient = PatientService.GetPatient(issueTreatment.PatientId);

            if (patient == null)
            {
                return NotFound("Patient not found.");
            }

            await this.ValidateResourcePermissionAsync(patient);

            avShuntService.DeleteIssueTreatment(id);

            return Ok();
        }
    }
}
