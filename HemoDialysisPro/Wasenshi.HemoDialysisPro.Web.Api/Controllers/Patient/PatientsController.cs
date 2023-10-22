using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using ServiceStack.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Wasenshi.AuthPolicy;
using Wasenshi.AuthPolicy.Attributes;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Services;
using Wasenshi.HemoDialysisPro.Services.Core.Interfaces;
using Wasenshi.HemoDialysisPro.Services.Interfaces;
using Wasenshi.HemoDialysisPro.Share;
using Wasenshi.HemoDialysisPro.ViewModels;
using Wasenshi.HemoDialysisPro.ViewModels.Base;
using Wasenshi.HemoDialysisPro.Web.Api.Controllers.Utils;
using Wasenshi.HemoDialysisPro.Web.Api.Utils;
using Wasenshi.HemoDialysisPro.Web.Api.Utils.ModelSearchers;
using Wasenshi.HemoDialysisPro.Models.Settings;
using Wasenshi.HemoDialysisPro.Models.ConfigExtesions;
using Wasenshi.HemoDialysisPro.Utils;
using Microsoft.Extensions.Configuration;
using Wasenshi.HemoDialysisPro.Models.TimezoneUtil;

namespace Wasenshi.HemoDialysisPro.Web.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PatientsController : ControllerBase
    {
        private readonly IPatientService _patientService;
        private readonly IVerifyPatientService verifyPatient;
        private readonly IUserInfoService userInfo;
        private readonly IHemoService hemoService;
        private readonly IMapper _mapper;
        private readonly IRedisClient redis;
        private readonly IWritableOptions<GlobalSetting> setting;

        private readonly TimeZoneInfo tz;

        public PatientsController(
            IPatientService patientService,
            IVerifyPatientService verifyPatient,
            IUserInfoService userInfo,
            IHemoService hemoService,
            IMapper mapper,
            IRedisClient redis,
            IConfiguration config,
            IWritableOptions<GlobalSetting> setting)
        {
            _patientService = patientService;
            this.verifyPatient = verifyPatient;
            this.userInfo = userInfo;
            this.hemoService = hemoService;
            _mapper = mapper;
            this.redis = redis;
            this.setting = setting;
            tz = TimezoneUtils.GetTimeZone(config["TIMEZONE"]);
        }

        [HttpGet]
        public IActionResult GetAll(int page = 1, int limit = 25, [FromQuery] List<string> orderBy = null, Guid? doctorId = null, string where = null)
        {
            Action<IOrderer<Patient>> orders = null;
            if (orderBy?.Count > 0)
            {
                orders = orderer =>
                {
                    foreach (var item in orderBy)
                    {
                        var split = item.Split('_');
                        string key = split[0];
                        bool desc = split.Length > 1 && split[1] == "desc";
                        Order(key, desc, orderer);
                    }
                    void Order(string key, bool desc, IOrderer<Patient> orderer)
                    {
                        switch (key.ToLower())
                        {
                            case "id":
                                orderer.OrderBy(p => p.Id, desc);
                                break;
                            case "hn":
                                orderer.OrderBy(p => p.HospitalNumber, desc);
                                break;
                            case "name":
                                orderer.OrderBy(p => p.Name, desc);
                                break;
                            case "birthdate":
                                orderer.OrderBy(p => p.BirthDate, desc);
                                break;
                            case "gender":
                                orderer.OrderBy(p => p.Gender, desc);
                                break;
                            case "doctor":
                                orderer.OrderBy(p => p.DoctorId, desc);
                                break;
                            case "admission":
                                orderer.OrderBy(p => p.Admission, desc);
                                break;
                            case "unit":
                                orderer.OrderBy(p => p.UnitId, desc);
                                break;
                            case "coverage":
                                orderer.OrderBy(p => p.CoverageScheme, desc);
                                break;
                        }
                    }
                };
            }

            Expression<Func<Patient, bool>> whereCondition = null;
            if (!string.IsNullOrWhiteSpace(where))
            {
                whereCondition = new PatientSearch().GetWhereCondition(where);
                if (whereCondition == null)
                {
                    return Ok(new PageView<PatientViewModel>
                    {
                        Data = Enumerable.Empty<PatientViewModel>(),
                        Total = 0
                    });
                }
            }
            Page<Patient> patients = null;
            if (this.IsDoctorAndSeeOwnPatientOnly(setting.Value))
            {
                doctorId = User.GetUserIdAsGuid();
            }
            if (doctorId.HasValue)
            {
                var units = userInfo.GetUserUnits(doctorId.Value).ToList();
                Expression<Func<Patient, bool>> unitFilter = x => x.DoctorId == doctorId.Value && units.Contains(x.UnitId);
                whereCondition = unitFilter.AndAlso(whereCondition);
                patients = _patientService.GetDoctorPatients(doctorId.Value, page, limit, orders, whereCondition);
            }
            else
            {
                Expression<Func<Patient, bool>> unitFilter = User.GetUnitFilter<Patient>(x => x.UnitId);
                whereCondition = unitFilter.AndAlso(whereCondition);
                patients = _patientService.GetAllPatients(page, limit, orders, whereCondition);
            }

            var count = patients.Total;
            var data = _mapper.Map<IEnumerable<Patient>, IEnumerable<PatientViewModel>>(patients.Data);

            if (doctorId.HasValue)
            {
                var tzNow = TimeZoneInfo.ConvertTime(new DateTimeOffset(DateTime.UtcNow, TimeSpan.Zero), tz).AsDate();
                foreach (var patient in data)
                {
                    patient.TotalThisMonth = hemoService.CountAll(x => x.Patient.Id == patient.Id && x.Record.CompletedTime.Value.Year == tzNow.Year && x.Record.CompletedTime.Value.Month == tzNow.Month);
                }
            }

            data.SetSessionForPatients(redis);

            return Ok(new PageView<PatientViewModel>
            {
                Data = data,
                Total = count
            });
        }

        [HttpGet("unit/{unitId}")]
        public IActionResult GetAllByUnit(int unitId, int page = 1, int limit = 25)
        {
            Expression<Func<Patient, bool>> whereCondition = null;
            if (this.IsDoctorAndSeeOwnPatientOnly(setting.Value))
            {
                var doctorId = User.GetUserIdAsGuid();
                whereCondition = x => x.DoctorId == doctorId;
            }
            var patients = _patientService.GetUnitPatients(unitId, page, limit, whereCondition);

            var count = patients.Total;
            var data = _mapper.Map<IEnumerable<Patient>, IEnumerable<PatientViewModel>>(patients.Data);

            data.SetSessionForPatients(redis);

            return Ok(new PageView<PatientViewModel>
            {
                Data = data,
                Total = count
            });
        }

        [HttpPost]
        public IActionResult CreatePatient([FromBody] CreatePatientViewModel patient)
        {
            Patient newPatient = _mapper.Map<Patient>(patient);

            if (!verifyPatient.VerifyUnit(User, newPatient))
            {
                return Forbid();
            }

            try
            {
                _patientService.CreateNewPatient(newPatient);

                return Created($"{newPatient.Id}", _mapper.Map<PatientViewModel>(newPatient));
            }
            catch (PatientService.PatientException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public IActionResult GetPatient(string id)
        {
            var patient = _patientService.GetPatient(id);
            if (patient == null)
            {
                return NotFound();
            }
            if (this.IsDoctorAndSeeOwnPatientOnly(setting.Value) &&
                (!patient.DoctorId.HasValue || patient.DoctorId.Value != User.GetUserIdAsGuid()))
            {
                return Forbid();
            }

            var result = _mapper.Map<PatientViewModel>(patient);
            return Ok(result);
        }

        [HttpPost("{id}")]
        public async Task<IActionResult> EditPatientAsync(string id, EditPatientViewModel patient)
        {
            Patient oldPatient = _patientService.GetPatient(id);

            if (oldPatient == null)

            {
                return NotFound("Patient not found.");
            }

            await this.ValidateResourcePermissionAsync(oldPatient);

            // change id
            bool changeId = id != patient.Id;

            Patient editPatient = changeId ? _mapper.Map<Patient>(patient) : _mapper.Map(patient, oldPatient);

            if (!verifyPatient.VerifyUnit(User, editPatient))
            {
                return Forbid();
            }

            try
            {
                if (changeId)
                {
                    string newId = editPatient.Id;
                    editPatient.Id = id;
                    _patientService.UpdatePatient(editPatient, newId);
                }
                else
                {
                    _patientService.UpdatePatient(editPatient);
                }

                return Ok();
            }
            catch (PatientService.PatientException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [PermissionAuthorize(Permissions.Patient.DELETE)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePatientAsync(string id)
        {
            Patient patient = _patientService.GetPatient(id);

            if (patient == null)
            {
                return NotFound("Patient not found.");
            }

            await this.ValidateResourcePermissionAsync(patient);

            _patientService.DeletePatient(id);

            return Ok();
        }

        [HttpGet("{id}/history")]
        public IActionResult GetPatientHistory(string id)
        {
            var patient = _patientService.GetPatient(id);
            if (patient == null)
            {
                return NotFound();
            }
            if (this.IsDoctorAndSeeOwnPatientOnly(setting.Value) &&
                (!patient.DoctorId.HasValue || patient.DoctorId.Value != User.GetUserIdAsGuid()))
            {
                return Forbid();
            }

            var history = _patientService.GetPatientHistory(id);

            var result = _mapper.Map<IEnumerable<PatientHistoryViewModel>>(history);
            return Ok(result);
        }

        [HttpPost("{id}/history")]
        public IActionResult UpdatePatientHistory(string id, [FromBody]IEnumerable<PatientHistoryViewModel> list)
        {
            var patient = _patientService.GetPatient(id);
            if (patient == null)
            {
                return NotFound();
            }
            if (this.IsDoctorAndSeeOwnPatientOnly(setting.Value) &&
                (!patient.DoctorId.HasValue || patient.DoctorId.Value != User.GetUserIdAsGuid()))
            {
                return Forbid();
            }

            var result = _patientService.UpdatePatientHistory(id, _mapper.Map<IEnumerable<PatientHistory>>(list));

            if (!result)
            {
                return NotFound();
            }

            return Ok();
        }

        // ========================= Settings =============================
        [HttpGet("setting")]
        public IActionResult GetSetting()
        {
            PatientSetting patientSetting = setting.Value.Patient ??= new();

            return Ok(patientSetting);
        }

        [PermissionAuthorize(Permissions.Patient.RULE)]
        [HttpPut("setting")]
        public async Task<IActionResult> SetSettingAsync(PatientSetting request)
        {

            setting.Update(x =>
            {
                x.Patient = request;
                redis.Set(Common.SEE_OWN_PATIENT_ONLY, x.Patient.DoctorCanSeeOwnPatientOnly);
            });

            return Ok();
        }
    }
}
