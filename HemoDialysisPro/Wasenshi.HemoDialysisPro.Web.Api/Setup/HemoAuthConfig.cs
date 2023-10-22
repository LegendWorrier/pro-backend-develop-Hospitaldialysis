using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Wasenshi.HemoDialysisPro.Web.Api.AuthPolicy.AuthHandlers;

namespace Wasenshi.HemoDialysisPro.Web.Api.Setup
{
    /// <summary>
    /// Additional authorization config specific for HemoDialysis App
    /// </summary>
    public static class HemoAuthConfig
    {
        public static IServiceCollection AddHemoAuthConfig(this IServiceCollection services, IConfiguration config)
        {
            services.AddTransient<IAuthorizationHandler, RoleAndPermissionHandler>();

            return services;
        }
    }
}
