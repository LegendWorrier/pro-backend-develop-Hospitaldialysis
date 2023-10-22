using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Wasenshi.AuthPolicy.Attributes;
using Wasenshi.HemoDialysisPro.Models.Settings;
using Wasenshi.HemoDialysisPro.Services.Interfaces;
using Wasenshi.HemoDialysisPro.Models.ConfigExtesions;
using static Wasenshi.HemoDialysisPro.Share.Permissions;
using System.Reflection;

namespace Wasenshi.HemoDialysisPro.Web.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UnitSettingController : ControllerBase
    {
        private readonly IAuthService authService;
        private readonly IWritableOptions<UnitSettings> unitSetting;
        private readonly IWritableOptions<GlobalSetting> setting;
        private readonly IConfiguration config;

        private static readonly IMapper UnitSettingMapper = new MapperConfiguration(c =>
        {
            c.CreateMap<UnitSettings, UnitSettings>();
        }).CreateMapper();

        public UnitSettingController(
            IAuthService authService,
            IWritableOptions<UnitSettings> unitSetting,
            IWritableOptions<GlobalSetting> setting,
            IConfiguration config)
        {
            this.authService = authService;
            this.unitSetting = unitSetting;
            this.setting = setting;
            this.config = config;
        }

        [PermissionAuthorize(UNIT_SETTING)]
        [HttpPost("{unitId}")]
        public IActionResult SetUnitSetting(int unitId, UnitSettings setting)
        {

            if (!authService.VerifyUnit(User, new[] { unitId }))
            {
                return Forbid();
            }

            unitSetting.Update(unitId.ToString(), x => UnitSettingMapper.Map(setting, x));

            return Ok();
        }

        [HttpGet("{unitId}")]
        public IActionResult GetUnitSetting(int unitId)
        {
            UnitSettings defaultValue = unitSetting.Value;

            var targetUnit = unitSetting.Get(unitId.ToString());
            if (targetUnit == null) return Ok(defaultValue);

            PropertyInfo[] properties = typeof(UnitSettings).GetProperties();
            foreach (var item in properties)
            {
                if (item.GetValue(targetUnit) == null)
                {
                    var defaultItem = item.GetValue(defaultValue);
                    item.SetValue(targetUnit, defaultItem);
                }
            }

            return Ok(targetUnit);
        }


    }
}
