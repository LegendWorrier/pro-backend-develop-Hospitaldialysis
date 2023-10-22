using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weikio.PluginFramework.Abstractions;

namespace Wasenshi.HemoDialysisPro.PluginIntegrate
{
    public static class PluginExtensions
    {
        public static object Create(this Plugin plugin, IServiceProvider serviceProvider, params object[] parameters)
        {
            return ActivatorUtilities.CreateInstance(serviceProvider, plugin, parameters);
        }

        public static T Create<T>(this Plugin plugin, IServiceProvider serviceProvider, params object[] parameters) where T : class
        {
            return ActivatorUtilities.CreateInstance(serviceProvider, plugin, parameters) as T;
        }
    }
}
