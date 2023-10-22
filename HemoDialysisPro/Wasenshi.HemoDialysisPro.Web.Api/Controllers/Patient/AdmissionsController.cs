using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Wasenshi.AuthPolicy;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Services;
using Wasenshi.HemoDialysisPro.Services.Core.Interfaces;
using Wasenshi.HemoDialysisPro.Services.Interfaces;
using Wasenshi.HemoDialysisPro.ViewModels;
using Wasenshi.HemoDialysisPro.ViewModels.Base;

namespace Wasenshi.HemoDialysisPro.Web.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdmissionsController : ControllerBase
    {
        private readonly IAdmissionService _admissionService;
        private readonly IVerifyPatientService verifyPatient;
        private readonly IUserInfoService userInfo;
        private readonly IMapper _mapper;

        public AdmissionsController(IAdmissionService admissionService, IVerifyPatientService verifyPatient, IUserInfoService userInfo, IMapper mapper)
        {
            _admissionService = admissionService;
            this.verifyPatient = verifyPatient;
            this.userInfo = userInfo;
            _mapper = mapper;
        }

        [HttpGet("patient/{patientId}/active")]
        public IActionResult GetActiveByPatient(string patientId)
        {
            var admit = _admissionService.FindAdmission(x => x.PatientId == patientId && x.Discharged == null);

            return Ok(_mapper.Map<AdmissionViewModel>(admit));
        }

        [HttpGet("patient/{patientId}")]
        public IActionResult GetAllByPatient(string patientId, int page = 1, int limit = 25)
        {
            var admits = _admissionService.GetAdmissionForPatient(patientId, page, limit);

            var count = admits.Total;
            var data = _mapper.Map<IEnumerable<Admission>, IEnumerable<AdmissionViewModel>>(admits.Data);

            return Ok(new PageView<AdmissionViewModel>
            {
                Data = data,
                Total = count
            });
        }

        [HttpPost]
        public IActionResult CreateAdmit([FromBody] CreateAdmissionViewModel admit)
        {
            Admission newAdmit = _mapper.Map<Admission>(admit);

            if (!verifyPatient.VerifyUnit(User, newAdmit.PatientId))
            {
                return Forbid();
            }

            try
            {
                _admissionService.CreateNewAdmission(newAdmit);

                return Created($"{newAdmit.Id}", _mapper.Map<AdmissionViewModel>(newAdmit));
            }
            catch (PatientService.PatientException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public IActionResult GetAdmit(Guid id)
        {
            var admit = _admissionService.GetAdmission(id);
            if (admit == null)
            {
                return NotFound();
            }

            var result = _mapper.Map<AdmissionViewModel>(admit);
            return Ok(result);
        }

        [HttpPost("{id}")]
        public async Task<IActionResult> EditAdmissionAsync(Guid id, AdmissionViewModel admit)
        {
            Admission oldAdmit = _admissionService.GetAdmission(id);

            if (oldAdmit == null)
            {
                return NotFound("Admission not found.");
            }

            if (!verifyPatient.VerifyUnit(User, oldAdmit.PatientId))
            {
                return Forbid();
            }

            try
            {
                var mapped = _mapper.Map<Admission>(admit);
                mapped.PatientId = oldAdmit.PatientId;
                _admissionService.UpdateAdmission(mapped);

                return Ok();
            }
            catch (PatientService.PatientException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAdmissionAsync(Guid id)
        {
            Admission admit = _admissionService.GetAdmission(id);

            if (admit == null)
            {
                return NotFound("Admission not found.");
            }

            await this.ValidateResourcePermissionAsync(admit);

            _admissionService.DeleteAdmission(id);

            return Ok();
        }
    }
}
