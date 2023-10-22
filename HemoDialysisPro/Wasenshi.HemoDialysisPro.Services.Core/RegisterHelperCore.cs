using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Reflection;
using Wasenshi.HemoDialysisPro.PluginIntegrate;
using Wasenshi.HemoDialysisPro.Services.BusinessLogic;
using Wasenshi.HemoDialysisPro.Services.Core;
using Wasenshi.HemoDialysisPro.Services.Interfaces.Base;

namespace Wasenshi.HemoDialysisPro.Services
{
    public static class RegisterHelperCore
    {
        public static IServiceCollection RegisterServicesCore(this IServiceCollection services, params Assembly[] additional)
        {
            var serviceClasses = Assembly.GetExecutingAssembly().GetTypes()
                .Where(x => x.IsClass && typeof(IApplicationService).IsAssignableFrom(x)).ToList();
            if (additional.Any())
            {
                serviceClasses = serviceClasses.Concat(additional.SelectMany(x => x.GetTypes()).Where(x => x.IsClass && typeof(IApplicationService).IsAssignableFrom(x))).ToList();
            }

            foreach (var type in serviceClasses)
            {
                var interfaces = type.GetInterfaces();
                var @interface = interfaces.First(x => typeof(IApplicationService).IsAssignableFrom(x));
                services.AddScoped(@interface, type);
            }

            return services;
        }

        public static IServiceCollection ConfigPluginSystem(this IServiceCollection services)
        {
            return services.ConfigPluginSystem<HemoPro, StatProcessor, StatInfoProcessor>();
        }
    }
}
