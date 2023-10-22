using AutoMapper;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using ServiceStack.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using Wasenshi.AuthPolicy.Attributes;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Models.Settings;
using Wasenshi.HemoDialysisPro.Services.Interfaces;
using Wasenshi.HemoDialysisPro.Share;
using Wasenshi.HemoDialysisPro.Models.ConfigExtesions;
using Wasenshi.HemoDialysisPro.Models.TimezoneUtil;
using Wasenshi.HemoDialysisPro.ViewModels;
using Wasenshi.HemoDialysisPro.Web.Api.BackgroundTasks;
using Wasenshi.HemoDialysisPro.Web.Api.Controllers.Utils;
using Wasenshi.HemoDialysisPro.Utils;

namespace Wasenshi.HemoDialysisPro.Web.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ShiftController : ControllerBase
    {
        private readonly IShiftService shiftService;
        private readonly IMapper mapper;
        private readonly IMasterDataService masterData;
        private readonly IAuthService auth;
        private readonly IPatientService patientService;
        private readonly IRedisClient redis;
        private readonly IBackgroundJobClient backgroundJob;
        private readonly IWritableOptions<GlobalSetting> setting;
        private readonly IConfiguration config;

        private readonly TimeZoneInfo tz;

        private static readonly IMapper ShiftInfoMapper = new MapperConfiguration(c =>
        {
            c.CreateMap<UnitShift, ShiftInfoViewModel>()
                .ForMember(x => x.UnitId, c => c.MapFrom(x => x.Id));
            c.CreateMap<ScheduleSection, ScheduleSectionViewModel>();
            c.CreateMap<TimeOnly, int>().ConstructUsing(d => d.Hour * 60 + d.Minute);
        }).CreateMapper();

        public ShiftController(
            IShiftService shiftService,
            IMapper mapper,
            IMasterDataService masterData,
            IAuthService auth,
            IPatientService patientService,
            IRedisClient redis,
            IBackgroundJobClient backgroundJob,
            IWritableOptions<GlobalSetting> setting,
            IConfiguration config)
        {
            this.shiftService = shiftService;
            this.mapper = mapper;
            this.masterData = masterData;
            this.auth = auth;
            this.patientService = patientService;
            this.redis = redis;
            this.backgroundJob = backgroundJob;
            this.setting = setting;
            this.config = config;

            tz = TimezoneUtils.GetTimeZone(config.GetValue<string>("TIMEZONE"));
        }

        [HttpGet("{unitId}/info")]
        public IActionResult GetCurrentShiftInfo(int unitId)
        {
            var unitShift = redis.GetUnitShift(unitId);
            if (unitShift == null)
            {
                return NotFound(unitId);
            }

            return Ok(ShiftInfoMapper.Map<ShiftInfoViewModel>(unitShift));
        }

        [Authorize(Roles = Roles.HeadNurseOnly)]
        [HttpPut("{unitId}/start-next")]
        public IActionResult StartNextRound(int unitId)
        {
            if (!redis.UnitExists(unitId))
            {
                return BadRequest($"the unit does not exist.");
            }

            this.ValidateUnitHeadOrInCharged(unitId, masterData, shiftService, redis);

            backgroundJob.StartNextRound(unitId);

            return Ok();
        }

        [HttpGet("list")]
        public IActionResult GetShiftsHistoryList()
        {
            var result = shiftService.GetHistoryList(User.GetUnitList());

            return Ok(result);
        }

        [HttpGet]
        public IActionResult GetShifts(DateOnly? month = null)
        {
            if (!month.HasValue)
            {
                month = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz));
            }
            var result = shiftService.GetAllUnitShifts(month, User.GetUnitList().ToArray());

            var viewModel = mapper.Map<ShiftResultViewModel>(result);
            viewModel.Month = month.Value;
            foreach (var item in viewModel.Users)
            {
                item.Month = month.Value;
            }

            return Ok(viewModel);
        }

        [HttpGet("{unitId}")]
        public IActionResult GetShiftByUnitId(int unitId, DateOnly? month = null)
        {
            if (!month.HasValue)
            {
                month = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz));
            }
            var result = shiftService.GetShiftForUnit(unitId, month);

            var viewModel = mapper.Map<ShiftResultViewModel>(result);
            viewModel.Month = month.Value;
            viewModel.UnitId = unitId;

            return Ok(viewModel);
        }

        [HttpGet("self")]
        public IActionResult GetShiftSelf(DateOnly? month = null)
        {
            if (!month.HasValue)
            {
                month = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz));
            }
            var result = shiftService.GetShiftForUser(new Guid(User.GetUserId()), month);

            var viewModel = mapper.Map<ShiftResultViewModel>(result);
            viewModel.Month = month.Value;

            return Ok(viewModel);
        }

        [HttpGet("{unitId}/incharge")]
        public IActionResult GetIncharge(int unitId, DateOnly? month = null)
        {
            var result = shiftService.GetInchargeList(unitId, month);

            return Ok(mapper.Map<IEnumerable<InchargeViewModel>>(result));
        }

        [HttpGet("{unitId}/incharge/check")]
        public IActionResult IsIncharge(int unitId)
        {
            var result = shiftService.IsIncharge(new Guid(User.GetUserId()), unitId, redis.GetUnitShift(unitId).CurrentSection);

            return Ok(result);
        }

        [HttpPost()]
        public IActionResult CreateOrUpdateShifts([FromBody] ShiftsEditViewModel request, [FromQuery] DateOnly? month = null)
        {
            var unitList = request.ShiftSlots.Where(x => x.UnitId != null).Select(x => x.UnitId.Value).Distinct().ToList();
            if (!VerifyEditableForUnits(unitList))
            {
                return Forbid();
            }

            if (!month.HasValue)
            {
                month = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz));
            }

            var slot = mapper.Map<IEnumerable<ShiftSlot>>(request.ShiftSlots);

            var suspended = mapper.Map<IEnumerable<UserShift>>(request.SuspendedList);
            var _ = shiftService.CreateOrUpdateShift(month.Value, slot, suspended);
            // not used. Let FE refresh and get updated itself.

            return Ok();
        }

        [HttpPost("self")]
        public IActionResult CreateOrUpdateShiftsForSelf([FromBody] ShiftsEditViewModel request, [FromQuery] DateOnly? month = null)
        {
            var unitList = request.ShiftSlots.Where(x => x.UnitId != null).Select(x => x.UnitId.Value).Distinct().ToList();
            var userId = new Guid(User.GetUserId());
            if (request.ShiftSlots.Any(x => x.UserId != userId) || !auth.VerifyUnit(User, unitList))
            {
                return Forbid();
            }

            if (!month.HasValue)
            {
                month = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz));
            }

            var slot = mapper.Map<IEnumerable<ShiftSlot>>(request.ShiftSlots);
            IEnumerable<UserShift> suspended;
            if (request.IsSuspended.HasValue && request.IsSuspended.Value)
            {
                suspended = new[] { new UserShift { Suspended = true, UserId = userId } };
            }
            else
            {
                suspended = Enumerable.Empty<UserShift>();
            }
            shiftService.CreateOrUpdateShift(month.Value, slot, suspended);

            return Ok(mapper.Map<IEnumerable<ShiftSlotViewModel>>(slot));
        }

        [HttpPost("incharges")]
        public IActionResult CreateOrUpdateIncharges([FromBody] IEnumerable<InchargeViewModel> request)
        {
            var unitList = request.Select(x => x.UnitId).Distinct().ToList();
            if (!VerifyEditableForUnits(unitList))
            {
                return Forbid();
            }

            var incharges = mapper.Map<IEnumerable<ShiftIncharge>>(request);
            var result = shiftService.AddOrUpdateIncharge(incharges);

            return Ok(result);
        }

        [PermissionAuthorize(Permissions.SHIFTHISTORY)]
        [HttpPut("history/clear")]
        public IActionResult ClearShiftHistory(DateOnly? upperLimit = null)
        {
            var result = shiftService.ClearShiftHistory(upperLimit);

            return Ok(result);
        }

        [HttpGet("history/setting")]
        public IActionResult GetHistorySetting()
        {
            ShiftHistorySetting shiftHistory = setting.Value.ShiftHistory;

            return Ok(shiftHistory);
        }

        [PermissionAuthorize(Permissions.SHIFTHISTORY)]
        [HttpPut("history/setting")]
        public IActionResult SetHistorySetting(ShiftHistorySetting request)
        {
            setting.Update(x =>
            {
                x.ShiftHistory = request;
            });

            return Ok();
        }

        // ================= Util =================
        private bool VerifyEditableForUnits(IEnumerable<int> unitList)
        {
            if (!User.IsInRole(Roles.HeadNurse) && !User.IsInRole(Roles.Admin))
            {
                var userId = new Guid(User.GetUserId());
                bool forbid = !unitList.Any() && !masterData.IsUnitHead(userId);
                foreach (var unitId in unitList)
                {
                    if (!masterData.IsUnitHead(userId, unitId))
                    {
                        forbid = true;
                        break;
                    }
                }
                if (forbid) return false;
            }
            if (!auth.VerifyUnit(User, unitList))
            {
                return false;
            }

            return true;
        }
    }
}
