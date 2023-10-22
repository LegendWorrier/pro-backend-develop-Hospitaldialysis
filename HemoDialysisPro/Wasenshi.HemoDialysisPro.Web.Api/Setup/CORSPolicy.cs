using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Threading.Tasks;

namespace Wasenshi.HemoDialysisPro.Web.Api.Setup
{
    public static class CorsConfig
    {
        public static IServiceCollection AddCustomCors(this IServiceCollection services)
        {
            services.AddSingleton<ICorsPolicyProvider, CorsPolicyProvider>();
            services.AddCors();
            return services;
        }

        public class CorsPolicyProvider : ICorsPolicyProvider
        {
            public CorsPolicyProvider(IConfiguration config)
            {
                Config = config;
                ReadOrigins();
            }

            public IConfiguration Config { get; }
            public string[] AllowedOrigins { get; set; }

            private void ReadOrigins()
            {
                var allowedOrigins = Config.GetValue<string>("Origin");

                AllowedOrigins = allowedOrigins.Split(",").Select(x => x.Trim()).ToArray();
            }

            public Task<CorsPolicy> GetPolicyAsync(HttpContext context, string policyName)
            {
                string[] origins;
                if (AllowedOrigins.FirstOrDefault() == "*")
                {
                    origins = context.Request.Headers["Origin"];
                }
                else
                {
                    origins = AllowedOrigins;
                }
                var cosPolicy = new CorsPolicyBuilder()
                    .AllowCredentials()
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .SetIsOriginAllowedToAllowWildcardSubdomains()
                    .WithOrigins(origins)
                    .Build();
                return Task.FromResult(cosPolicy);
            }
        }
    }
}
