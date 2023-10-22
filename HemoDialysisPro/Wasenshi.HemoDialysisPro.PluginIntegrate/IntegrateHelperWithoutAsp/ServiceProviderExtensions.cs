using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weikio.PluginFramework.Abstractions;

namespace Wasenshi.HemoDialysisPro.PluginIntegrate
{
    public static class ServiceProviderExtensions
    {
        public static object Create(this IServiceProvider serviceProvider, Plugin plugin)
        {
            return ActivatorUtilities.CreateInstance(serviceProvider, plugin);
        }

        public static T Create<T>(this IServiceProvider serviceProvider, Plugin plugin) where T : class
        {
            return ActivatorUtilities.CreateInstance(serviceProvider, plugin) as T;
        }
    }
}
