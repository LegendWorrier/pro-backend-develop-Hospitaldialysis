using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Wasenshi.HemoDialysisPro.Repositories;
using Wasenshi.HemoDialysisPro.Services;
using Wasenshi.HemoDialysisPro.Web.Api.Helpers;

namespace Wasenshi.HemoDialysisPro.Web.Api
{
    public static class ServicesConfig
    {
        public static IServiceCollection AddScopes(this IServiceCollection services, IConfiguration configuration)
        {
            services.RegisterRepositories(true);
            services.RegisterServices();
            services.AddTransient<AuthHelper>();

            return services;
        }
    }
}
