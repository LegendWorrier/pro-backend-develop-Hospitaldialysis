using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceStack.Messaging;
using ServiceStack.Redis;
using System;
using System.Collections.Generic;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Services.Core.Interfaces;
using Wasenshi.HemoDialysisPro.Services.Interfaces;
using Wasenshi.HemoDialysisPro.ViewModels;

namespace Wasenshi.HemoDialysisPro.Web.Api.Controllers
{
    [Authorize(Roles = Roles.NursesOnly)]
    [Route("api/HemoDialysis/Records", Order = -2)]
    [ApiController]
    public class ProgressNoteController : PatientBaseController
    {
        private readonly IHemoService hemoService;
        private readonly IRecordService recordService;
        private readonly ICosignService cosignService;
        private readonly IShiftService shiftService;
        private readonly IMasterDataService masterData;
        private readonly IUserInfoService userInfo;
        private readonly IRedisClient redis;
        private readonly IMessageQueueClient message;
        private readonly IMapper mapper;

        public ProgressNoteController(
            IHemoService hemoService,
            IRecordService recordService,
            ICosignService cosignService,
            IPatientService patientService,
            IVerifyPatientService verifyPatientService,
            IScheduleService scheduleService,
            IShiftService shiftService,
            IMasterDataService masterData,
            IUserInfoService userInfo,
            IRedisClient redis,
            IMessageQueueClient message,
            IMapper mapper) : base(patientService, verifyPatientService, scheduleService)
        {
            this.hemoService = hemoService;
            this.recordService = recordService;
            this.cosignService = cosignService;
            this.shiftService = shiftService;
            this.masterData = masterData;
            this.userInfo = userInfo;
            this.redis = redis;
            this.message = message;
            this.mapper = mapper;
        }

        [HttpGet("{hemoId}/progress-note")]
        public IActionResult GetAllProgressNoteRecordsForHemosheet(Guid hemoId)
        {
            //var hemosheet = hemoService.GetHemodialysisRecord(hemoId);

            //if (FindPatient(hemosheet?.PatientId) == null)
            //{
            //    return NotFound();
            //}

            IEnumerable<ProgressNote> records = recordService.GetProgressNotesByHemoId(hemoId);

            IEnumerable<ProgressNoteViewModel> result =
                mapper.Map<IEnumerable<ProgressNote>, IEnumerable<ProgressNoteViewModel>>(records);

            return Ok(result);
        }

        [HttpGet("progress-note/{id}")]
        public IActionResult GetProgressNote(Guid id)
        {
            var result = recordService.GetProgressNote(id);

            if (result == null)
            {
                return NotFound();
            }

            return Ok(mapper.Map<ProgressNoteViewModel>(result));
        }

        [HttpPost("progress-note")]
        public IActionResult CreateNewProgressNote([FromBody] ProgressNoteViewModel record)
        {
            var newRecord = mapper.Map<ProgressNote>(record);

            var result = recordService.CreateProgressNote(newRecord);

            return Ok(mapper.Map<ProgressNoteViewModel>(result));
        }

        [HttpPost("progress-note/{id}")]
        public IActionResult UpdateProgressNote(Guid id, [FromBody] ProgressNoteViewModel record)
        {
            record.Id = id;

            var original = recordService.GetProgressNote(id);

            var editRecord = mapper.Map(record, original);
            var result = recordService.UpdateProgressNote(editRecord);

            if (!result)
            {
                return NotFound();
            }

            return Ok(mapper.Map<ProgressNoteViewModel>(editRecord));
        }

        [HttpDelete("progress-note/{id}")]
        public IActionResult DeleteProgressNote(Guid id)
        {
            var result = recordService.DeleteProgressNote(id);
            if (!result)
            {
                return NotFound();
            }

            return Ok();
        }

        [HttpPut("progress-note/swap/{first}/{second}")]
        public IActionResult SwapProgressNote(Guid first, Guid second)
        {
            var result = recordService.SwapProgressNoteOrder(first, second);

            if (!result)
            {
                return NotFound();
            }

            return Ok();
        }

    }
}
