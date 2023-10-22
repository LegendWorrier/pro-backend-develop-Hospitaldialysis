using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Models.Stat;
using Wasenshi.HemoDialysisPro.PluginBase;
using Wasenshi.HemoDialysisPro.Services.Interfaces;
using Wasenshi.HemoDialysisPro.PluginIntegrate;
using Wasenshi.HemoDialysisPro.ViewModels;
using Serilog;
using System.Threading.Tasks;
using Wasenshi.HemoDialysisPro.PluginBase.Stats;

namespace Wasenshi.HemoDialysisPro.Web.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StatController : ControllerBase
    {
        private readonly IStatService statService;
        private readonly IEnumerable<IStatHandler> statPlugins;
        private readonly IMapper mapper;

        public StatController(IStatService statService, IEnumerable<IStatHandler> statPlugins, IMapper mapper)
        {
            this.statService = statService;
            this.statPlugins = statPlugins;
            this.mapper = mapper;
        }

        [HttpGet("assessments/{patientId?}")]
        public IActionResult GetAssessmentStat([FromQuery] string duration, [FromQuery] DateTimeOffset? pointOfTime = null, string patientId = null, [FromQuery] int? unitId = null)
        {
            DateTime? pt = null;
            if (pointOfTime.HasValue)
            {
                pt = pointOfTime.Value.UtcDateTime;
            }
            TableResult<int> result = statService.GetAssessmentStat(duration, pt, patientId, unitId);

            return Ok(mapper.Map<TableResultViewModel<int>>(result));
        }


        [HttpGet("dialysis/{patientId?}")]
        public IActionResult GetDialysisStat([FromQuery] string duration, [FromQuery] DateTimeOffset? pointOfTime = null, string patientId = null, [FromQuery] int? unitId = null)
        {
            DateTime? pt = null;
            if (pointOfTime.HasValue)
            {
                pt = pointOfTime.Value.UtcDateTime;
            }
            TableResult<StatInfo> result = statService.GetDialysisStat(duration, pt, patientId, unitId);

            return Ok(mapper.Map<TableResultViewModel<StatInfo>>(result));
        }

        [HttpGet("lab")]
        public IActionResult GetLabStat([FromQuery] string duration, [FromQuery] DateTimeOffset? pointOfTime = null, [FromQuery] int? unitId = null)
        {
            DateTime? pt = null;
            if (pointOfTime.HasValue)
            {
                pt = pointOfTime.Value.UtcDateTime;
            }
            TableResult<StatInfo> result = statService.GetLabExamGlobalStat(duration, pt, unitId);

            return Ok(mapper.Map<TableResultViewModel<StatInfo>>(result));
        }

        [HttpGet("{statName}/{patientId?}")]
        public async Task<IActionResult> GetCustomStat(string statName, [FromQuery] string duration, [FromQuery] DateTimeOffset? pointOfTime = null, [FromQuery] int? unitId = null, string patientId = null)
        {
            DateTime? pt = null;
            if (pointOfTime.HasValue)
            {
                pt = pointOfTime.Value.UtcDateTime;
            }

            TableResult<object> result = await statPlugins.ExecutePlugins(async handler =>
            {
                var result = handler.GetStat(statName, duration, pt, unitId, patientId);
                return result ?? (TableResult<object>)null;
            }, e => Log.Error(e, "Plugin error on stat get."));

            if (result == null)
            {
                return NotFound();
            }

            return Ok(mapper.Map<TableResultViewModel<object>>(result));
        }

        [HttpGet("custom-stat-list")]
        public async Task<IActionResult> GetCustomStatList()
        {

            StatItem[] stats = await statPlugins.ExecutePlugins(async handler =>
            {
                var stats = handler.GetCustomStatList();
                return stats ?? (StatItem[])null;
            }, e => Log.Error(e, "Plugin error on stat get."));

            if (stats == null)
            {
                return Ok(Array.Empty<StatItem>());
            }

            return Ok(stats);
        }
    }
}
