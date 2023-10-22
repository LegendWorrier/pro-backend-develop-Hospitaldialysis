using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using Wasenshi.HemoDialysisPro.Web.Api.Helpers;

namespace Wasenshi.HemoDialysisPro.Web.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LanguageController : ControllerBase
    {
        [HttpPut("change/{lang}")]
        public IActionResult ChangeLanguage(string lang)
        {
            var targetCulture = CultureInfo.GetCultureInfo(lang);
            CultureInfo.CurrentCulture = targetCulture;
            CultureInfo.CurrentUICulture = targetCulture;
            Response.SetLangCookie();

            return Ok();
        }
    }
}
