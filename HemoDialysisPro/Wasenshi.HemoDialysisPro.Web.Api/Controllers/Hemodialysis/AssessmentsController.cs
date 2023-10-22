using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Services.Interfaces;
using Wasenshi.HemoDialysisPro.Share;
using Wasenshi.HemoDialysisPro.ViewModels;
using Wasenshi.HemoDialysisPro.Utils;
using Wasenshi.AuthPolicy.Attributes;
using System.Net.Http;
using System.Net.Http.Json;
using FluentHttpClient;

namespace Wasenshi.HemoDialysisPro.Web.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AssessmentsController : PatientBaseController
    {
        private readonly IAssessmentService assessmentService;
        private readonly IHemoService hemoService;
        private readonly IConfiguration config;
        private readonly IHttpClientFactory clientFactory;
        private readonly IMapper mapper;

        public AssessmentsController(
            IAssessmentService assessmentService,
            IHemoService hemoService,
            IPatientService patientService,
            IVerifyPatientService verifyPatientService,
            IScheduleService scheduleService,
            IConfiguration config,
            IHttpClientFactory clientFactory,
            IMapper mapper) : base(patientService, verifyPatientService, scheduleService)
        {
            this.assessmentService = assessmentService;
            this.hemoService = hemoService;
            this.config = config;
            this.clientFactory = clientFactory;
            this.mapper = mapper;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var result = assessmentService.GetAllAssessments();
            var mapped = mapper.Map<IEnumerable<AssessmentViewModel>>(result);
            return Ok(mapped);
        }

        [HttpGet("group")]
        public IActionResult GetAllGroups()
        {
            var result = assessmentService.GetAllAssessmentGroups();
            var mapped = mapper.Map<IEnumerable<AssessmentGroupViewModel>>(result);
            return Ok(mapped);
        }

        [PermissionAuthorize(Permissions.ASSESSMENT)]
        [HttpPost("group")]
        public IActionResult CreateAssessmentGroup([FromBody] CreateAssessmentGroupViewModel group)
        {
            var newGroup = mapper.Map<AssessmentGroup>(group);
            assessmentService.AddGroup(newGroup);

            return Created($"{newGroup.Id}", mapper.Map<AssessmentGroupViewModel>(newGroup));
        }

        [PermissionAuthorize(Permissions.ASSESSMENT)]
        [HttpPost("group/{id}")]
        public IActionResult UpdateAssessmentGroup(int id, EditAssessmentGroupViewModel group)
        {
            group.Id = id;

            var original = assessmentService.GetGroup(id);

            var editGroup = mapper.Map(group, original);
            var result = assessmentService.UpdateGroup(editGroup);

            if (!result)
            {
                return NotFound();
            }

            return Ok(mapper.Map<AssessmentGroupViewModel>(editGroup));
        }

        [PermissionAuthorize(Permissions.ASSESSMENT)]
        [HttpDelete("group/{id}")]
        public IActionResult DeleteAssessmentGroup(int id)
        {
            assessmentService.RemoveGroup(id);

            return Ok();
        }

        [PermissionAuthorize(Permissions.ASSESSMENT)]
        [HttpPost]
        public IActionResult CreateAssessment([FromBody] CreateAssessmentViewModel assessment)
        {
            var newAssessment = mapper.Map<Assessment>(assessment);
            assessmentService.AddAssessment(newAssessment);

            return Created($"{newAssessment.Id}", mapper.Map<AssessmentViewModel>(newAssessment));
        }

        [PermissionAuthorize(Permissions.ASSESSMENT)]
        [HttpPost("{id}")]
        public IActionResult UpdateAssessment(long id, EditAssessmentViewModel assessment)
        {
            assessment.Id = id;

            var original = assessmentService.GetAssessment(id);

            var editAssessment = mapper.Map(assessment, original);
            var result = assessmentService.UpdateAssessment(editAssessment);

            if (!result)
            {
                return NotFound();
            }

            return Ok(mapper.Map<AssessmentViewModel>(editAssessment));
        }

        [PermissionAuthorize(Permissions.ASSESSMENT)]
        [HttpDelete("{assessmentId}")]
        public IActionResult DeleteAssessment(long assessmentId)
        {
            assessmentService.RemoveAssessment(assessmentId);

            return Ok();
        }

        [PermissionAuthorize(Permissions.ASSESSMENT)]
        [HttpPost("reorder/{firstId}/{secondId}")]
        public Task<IActionResult> ReorderAssessments(long firstId, long secondId)
        {
            assessmentService.ReorderAssessments(firstId, secondId);
            return Task.FromResult<IActionResult>(Ok());
        }
        [PermissionAuthorize(Permissions.ASSESSMENT)]
        [HttpPost("group/reorder/{firstId}/{secondId}")]
        public Task<IActionResult> ReorderAssessmentGroups(int firstId, int secondId)
        {
            assessmentService.ReorderGroups(firstId, secondId);
            return Task.FromResult<IActionResult>(Ok());
        }

        [HttpGet("hemosheet/{hemosheetId}/items")]
        public IActionResult GetAssessmentItems(Guid hemosheetId)
        {
            IEnumerable<AssessmentItem> result = assessmentService.GetHemosheetAssessmentItems(hemosheetId);

            return Ok(mapper.Map<IEnumerable<AssessmentItemViewModel>>(result));
        }

        [Authorize(Roles = Roles.NotPN)]
        [HttpPost("hemosheet/{hemosheetId}/items")]
        public IActionResult AddOrUpdateItems(Guid hemosheetId, [FromBody] IEnumerable<AssessmentItemViewModel> items)
        {
            HemodialysisRecord hemosheet = hemoService.GetHemodialysisRecord(hemosheetId);
            if (hemosheet == null)
            {
                return NotFound();
            }

            if (!VerifyPatienService.VerifyUnit(User, hemosheet.PatientId))
            {
                return Forbid();
            }

            var itemList = mapper.Map<IEnumerable<AssessmentItem>>(items);
            int count = assessmentService.AddOrUpdateItems(hemosheetId, itemList);
            if (count == 0)
            {
                return NotFound();
            }

            return Ok(mapper.Map<IEnumerable<AssessmentItemViewModel>>(itemList));
        }

        [Authorize(Roles = Roles.NotPN)]
        [HttpDelete("items/{id}")]
        public IActionResult DeleteAssessmentItem(Guid id)
        {
            AssessmentItem item = assessmentService.GetItem(id);
            if (item == null)
            {
                return NotFound();
            }
            HemodialysisRecord hemosheet = hemoService.GetHemodialysisRecord(item.HemosheetId);
            if (!VerifyPatienService.VerifyUnit(User, hemosheet.PatientId))
            {
                return Forbid();
            }

            var result = assessmentService.RemoveItem(id);
            if (!result)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, "Failed to delete assessment item.");
            }

            return Ok();
        }

        // ========================= Root Admin ==================================

        [Authorize(Roles = Roles.PowerAdmin)]
        [HttpGet("export")]
        public async Task<IActionResult> ExportCurrent(bool hasReassessment)
        {
            var (assessments, groups) = assessmentService.ExportAllAssessmentAndGroup();
            var result = new ExportAssessmentsData(assessments, groups, hasReassessment);

            var stream = new MemoryStream();
            await JsonSerializer.SerializeAsync(stream, result, new JsonSerializerOptions
            {
                WriteIndented = true,
            });
            stream.Position = 0;

            return File(stream, "application/setup", "export-assessment-setting.setup");
        }

        [Authorize(Roles = Roles.PowerAdmin)]
        [HttpPost("import")]
        public async Task<IActionResult> Import([FromForm] IFormCollection data)
        {
            var file = data.Files.GetFile("data");
            if (!(file?.ContentType.Contains("application/octet-stream", StringComparison.InvariantCultureIgnoreCase) ?? false))
            {
                return BadRequest("Unsupport file type.");
            }

            using var stream = file.OpenReadStream();
            ExportAssessmentsData importData;
            try
            {
                importData = await JsonSerializer.DeserializeAsync<ExportAssessmentsData>(stream, new JsonSerializerOptions
                {
                    IncludeFields = true,
                });
            }
            catch (Exception)
            {
                return BadRequest("Unexpected file type or the file is corrupted.");
            }

            if (importData.Setting != ExportAssessmentsData.CODE || importData.Version != ExportAssessmentsData.CURRENT_VERSION)
            {
                throw new AppException("FORMAT", "Wrong setting format or setting version.");
            }

            assessmentService.ImportAllAssessmentAndGroup(importData.Assessments, importData.Groups);

            // update reassessment config
            var client = clientFactory.CreateClient();
            var result = await client.PostAsJsonAsync(config["ConfigApiUrl"], new [] { new { Name = "hasReassessment", Value = importData.HasReassessment } });
            if (!result.IsSuccessStatusCode)
            {
                var response = await result.GetResponseStringAsync();
                return StatusCode(500, response);
            }

            return Ok();
        }

        internal struct ExportAssessmentsData
        {
            public const string CODE = "Assessment";
            public const short CURRENT_VERSION = 1; // Everytime assessment related data models got changed, we need to increase this.
            public ExportAssessmentsData(IEnumerable<Assessment> assessments, IEnumerable<AssessmentGroup> groups, bool hasRe)
            {
                Assessments = assessments;
                Groups = groups;
                HasReassessment = hasRe;
            }

            public string Setting { get; set; } = CODE;
            public short Version { get; set; } = CURRENT_VERSION;
            public DateTimeOffset ExportDate { get; set; } = DateTime.UtcNow;
            public bool HasReassessment { get; set; }
            public IEnumerable<AssessmentGroup> Groups { get; set; }
            public IEnumerable<Assessment> Assessments { get; set; }
        }
    }
}
