using Microsoft.Extensions.DependencyInjection;
using Wasenshi.HemoDialysisPro.Web.Api.Utils;

namespace Wasenshi.HemoDialysisPro.Web.Api.Setup
{
    public static class ExtraConfig
    {
        public static IServiceCollection ExtraSetup(this IServiceCollection services)
        {
            services.AddMvcCore(options =>
            {
                options.ModelBinderProviders.Insert(0, new DecodePathStringsBinderProvider());
            });
            services.AddDateOnlyTimeOnlyStringConverters();

            return services;
        }
    }
}
