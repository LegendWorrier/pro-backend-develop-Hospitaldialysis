using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Wasenshi.HemoDialysisPro.Services
{
    public static class RegisterHelper
    {
        public static IServiceCollection RegisterServices(this IServiceCollection services)
        {
            services.RegisterServicesCore(Assembly.GetExecutingAssembly());

            return services;
        }
    }
}
