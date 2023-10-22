using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Services;
using Wasenshi.HemoDialysisPro.Services.Interfaces;
using Wasenshi.HemoDialysisPro.ViewModels;
using Wasenshi.HemoDialysisPro.ViewModels.Base;
using Wasenshi.HemoDialysisPro.Utils;

namespace Wasenshi.HemoDialysisPro.Web.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MedHistoryController : PatientBaseController
    {
        private readonly IMedHistoryService medService;
        private readonly IMapper mapper;
        private readonly IConfiguration configuration;

        public MedHistoryController(
            IPatientService patientService,
            IVerifyPatientService verifyPatientService,
            IScheduleService scheduleService,
            IMedHistoryService medService,
            IMapper mapper,
            IConfiguration configuration) : base(patientService, verifyPatientService, scheduleService)
        {
            this.medService = medService;
            this.mapper = mapper;
            this.configuration = configuration;
        }

        [HttpGet]
        public IActionResult GetAllMedHistoryItems(int page = 1, int limit = 1000, [FromQuery] List<string> orderBy = null, [FromQuery] DateTimeOffset? filter = null)
        {
            Action<IOrderer<MedHistoryItem>> orders = null;
            if (orderBy?.Count > 0)
            {
                orders = (orderer) =>
                {
                    foreach (var item in orderBy)
                    {
                        var split = item.Split('_');
                        string key = split[0];
                        bool desc = split.Length > 1 && split[1] == "desc";
                        Order(key, desc, orderer);
                    }
                    void Order(string key, bool desc, IOrderer<MedHistoryItem> orderer)
                    {
                        switch (key.ToLower())
                        {
                            case "id":
                                orderer.OrderBy((h) => h.Id, desc);
                                break;
                            case "date":
                                orderer.OrderBy((h) => h.EntryTime, desc);
                                break;
                            case "amount":
                                orderer.OrderBy((h) => h.Quantity, desc);
                                break;
                            case "name":
                                orderer.OrderBy((h) => h.Medicine.Name, desc);
                                break;
                        }
                    }
                };
            }
            else
            {
                // Default ordering (patient then name then date)
                orders = (orderer) => orderer
                .OrderBy(x => x.PatientId)
                .OrderBy(x => x.Medicine.Name)
                .OrderBy(x => x.EntryTime, true);
            }

            Expression<Func<MedHistoryItem, bool>> whereCondition = null;
            DateTime limitDate;
            if (!filter.HasValue)
            {
                // Default limit within 3 months
                limitDate = DateTime.UtcNow.AddMonths(-2);
                limitDate = new DateTime(limitDate.Year, limitDate.Month, 1);
            }
            else
            {
                limitDate = filter.Value.UtcDateTime.Date;
            }
            whereCondition = x => x.EntryTime > limitDate;

            Page<MedHistoryItem> result = medService.GetAllMedHistory(page, limit, orders, whereCondition);

            var count = result.Total;
            var data = mapper.Map<IEnumerable<MedHistoryItem>, IEnumerable<MedHistoryItemViewModel>>(result.Data);

            return Ok(new PageView<MedHistoryItemViewModel>
            {
                Data = data,
                Total = count
            });
        }

        [HttpPost]
        public IActionResult CreateMedHistoryBatch(CreateMedHistoryBatchViewModel request)
        {
            if (FindPatient(request.PatientId) == null)
            {
                return NotFound();
            }

            IEnumerable<MedHistoryItem> newMeds = mapper.Map<IEnumerable<MedHistoryItem>>(request.Meds);

            IEnumerable<MedHistoryItem> result = medService.CreateMedHistoryBatch(request.PatientId, request.EntryTime.UtcDateTime, newMeds.ToList());

            return Ok(mapper.Map<IEnumerable<MedHistoryItem>, IEnumerable<MedHistoryItemViewModel>>(result));
        }

        [HttpPost("{id}")]
        public IActionResult UpdateMedHistoryItem(Guid id, MedHistoryItemViewModel MedHistoryItem)
        {
            if (FindPatient(MedHistoryItem.PatientId) == null)
            {
                return NotFound();
            }

            MedHistoryItem.Id = id;
            MedHistoryItem updateMed = mapper.Map<MedHistoryItem>(MedHistoryItem);

            MedHistoryItem result = medService.UpdateMedHistory(updateMed);

            return Ok(mapper.Map<MedHistoryItem, MedHistoryItemViewModel>(result));
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteMedHistoryItem(Guid id)
        {
            var MedHistoryItem = medService.GetMedHistory(id);

            if (FindPatient(MedHistoryItem.PatientId) == null)
            {
                return NotFound();
            }

            var result = medService.DeleteMedHistory(id);
            if (result)
            {
                return Ok();
            }

            return NotFound();
        }

        [HttpGet("{id}")]
        public IActionResult GetMedHistoryItem(Guid id)
        {
            var result = medService.GetMedHistory(id);
            if (result == null)
            {
                return NotFound();
            }

            return Ok(mapper.Map<MedHistoryItem, MedHistoryItemViewModel>(result));
        }

        [HttpGet("{patientId}/Detail")]
        public IActionResult GetMedHistoryDetailByPatientId(string patientId, [FromQuery] double? filter = null, [FromQuery] double? upperLimit = null)
        {
            DateTime? lowerD = filter?.UnixTimeStampToDateTime();
            DateTime? upperD = upperLimit?.UnixTimeStampToDateTime();
            var result = medService.GetMedHistoryByPatientId(patientId, null, lowerD, upperD);

            var data = mapper.Map<MedHistoryResult, MedHistoryResultViewModel>(result);

            return Ok(data);
        }

        [HttpGet("{patientId}/Overview")]
        public IActionResult GetMedOverviewByPatientId(string patientId)
        {
            var result = medService.GetMedOverviewByPatientId(patientId);

            var data = mapper.Map<MedOverview, MedOverviewViewModel>(result);

            return Ok(data);
        }

    }
}
