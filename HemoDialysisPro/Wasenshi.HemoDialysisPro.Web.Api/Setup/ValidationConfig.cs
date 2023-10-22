using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Wasenshi.HemoDialysisPro.Share;
using Wasenshi.HemoDialysisPro.ViewModels.Validation;
using Wasenshi.HemoDialysisPro.Web.Api.Utils;

namespace Wasenshi.HemoDialysisPro.Web.Api.Setup
{
    public static class ValidationConfig
    {
        public static IServiceCollection AddValidation(this IServiceCollection services)
        {
            services.AddMvc().AddDataAnnotationsLocalization();
            services.AddValidatorsFromAssemblyContaining<RegisterViewValidator>()
                    .AddFluentValidationAutoValidation()
                    .AddFluentValidationClientsideAdapters();

            var provider = services.BuildServiceProvider();
            var config = provider.GetRequiredService<IConfiguration>();
            if (config.GetValue<bool>("Locale:validator_error:customize"))
            {
                var localizer = provider.GetRequiredService<IStringLocalizer<ShareResource>>();
                ValidatorOptions.Global.LanguageManager = new ValidatorLocalizationManager(localizer, config);
            }

            return services;
        }
    }
}
