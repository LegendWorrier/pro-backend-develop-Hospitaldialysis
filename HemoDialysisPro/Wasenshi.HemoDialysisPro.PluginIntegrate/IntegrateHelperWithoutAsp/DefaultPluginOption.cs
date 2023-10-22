using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weikio.PluginFramework.Abstractions;

namespace Wasenshi.HemoDialysisPro.PluginIntegrate
{
    public class DefaultPluginOption
    {
        public Func<IServiceProvider, IEnumerable<Type>, Type> DefaultType { get; set; }
            = (serviceProvider, implementingTypes) => implementingTypes.FirstOrDefault();
    }
}
