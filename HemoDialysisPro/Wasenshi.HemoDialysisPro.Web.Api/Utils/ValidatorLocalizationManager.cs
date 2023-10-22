using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using System.Globalization;
using Wasenshi.HemoDialysisPro.Share;

namespace Wasenshi.HemoDialysisPro.Web.Api.Utils
{
    public class ValidatorLocalizationManager : FluentValidation.Resources.LanguageManager
    {
        public ValidatorLocalizationManager(IStringLocalizer<ShareResource> localizer, IConfiguration config)
        {
            AddTranslation("en", "NotNullValidator", "'{PropertyName}' is required.");
            AddTranslation("en-US", "NotNullValidator", "'{PropertyName}' is required.");
            AddTranslation("en-GB", "NotNullValidator", "'{PropertyName}' is required.");

            var cultureKey = config["CULTURE"];
            var culture = CultureInfo.GetCultureInfo(cultureKey);
            CultureInfo.CurrentCulture = culture;
            CultureInfo.CurrentUICulture = culture;
            AddTranslation(cultureKey, "NotEmptyValidator", localizer["NotEmptyValidator"]);
            AddTranslation(cultureKey, "NotNullValidator", localizer["NotNullValidator"]);
            AddTranslation(cultureKey, "MinimumLengthValidator", localizer["MinimumLengthValidator"]);
            AddTranslation(cultureKey, "EnumValidator", localizer["EnumValidator"]);
        }
    }
}
