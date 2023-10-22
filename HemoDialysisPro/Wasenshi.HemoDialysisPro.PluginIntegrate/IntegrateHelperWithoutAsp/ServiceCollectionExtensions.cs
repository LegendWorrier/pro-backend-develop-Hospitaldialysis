using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Weikio.PluginFramework.Abstractions;
using Weikio.PluginFramework.Catalogs;
using Weikio.PluginFramework.Configuration.Converters;
using Weikio.PluginFramework.Configuration.Providers;
using Weikio.PluginFramework.TypeFinding;

namespace Wasenshi.HemoDialysisPro.PluginIntegrate.IntegrateHelperWithoutAsp
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddPluginFrameworkCore(this IServiceCollection services, Action<PluginFrameworkOptions> configure = null)
        {
            if (configure != null)
            {
                services.Configure(configure);
            }

            services.AddHostedService<PluginFrameworkInitializer>();
            services.AddHostedService<PluginTaskScheduler>();
            services.AddTransient<PluginProvider>();

            services.TryAddTransient(typeof(IPluginCatalogConfigurationLoader), typeof(PluginCatalogConfigurationLoader));
            services.AddTransient(typeof(IConfigurationToCatalogConverter), typeof(FolderCatalogConfigurationConverter));
            services.AddTransient(typeof(IConfigurationToCatalogConverter), typeof(AssemblyCatalogConfigurationCoverter));

            services.AddConfiguration();

            services.AddSingleton(sp =>
            {
                var result = new List<Plugin>();
                var catalogs = sp.GetServices<IPluginCatalog>();

                foreach (var catalog in catalogs)
                {
                    var plugins = catalog.GetPlugins();

                    result.AddRange(plugins);
                }

                return result.AsEnumerable();
            });

            return services;
        }

        public static IServiceCollection AddPluginFrameworkCore<TType>(this IServiceCollection services, string dllPath = "") where TType : class
        {
            services.AddPluginFrameworkCore();

            if (string.IsNullOrWhiteSpace(dllPath))
            {
                var entryAssembly = Assembly.GetEntryAssembly();

                if (entryAssembly == null)
                {
                    dllPath = Environment.CurrentDirectory;
                }
                else
                {
                    dllPath = Path.GetDirectoryName(entryAssembly.Location);
                }
            }

            var typeFinderCriteria = TypeFinderCriteriaBuilder.Create()
                .AssignableTo(typeof(TType))
                .Build();

            var catalog = new FolderPluginCatalog(dllPath, typeFinderCriteria);
            services.AddPluginCatalog(catalog);

            services.AddPluginType<TType>();

            return services;
        }

        /// <summary>
        /// Add plugins from the IConfiguration.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> on which the plugins will be added.</param>
        /// <returns>This <see cref="IServiceCollection"/>.</returns>
        private static IServiceCollection AddConfiguration(this IServiceCollection services)
        {
            services.TryAddSingleton<IPluginCatalog>(serviceProvider =>
            {
                var options = serviceProvider.GetService<IOptions<PluginFrameworkOptions>>().Value;

                if (options.UseConfiguration == false)
                {
                    return new EmptyPluginCatalog();
                }

                // Grab all the IPluginCatalogConfigurationLoader implementations to load catalog configurations.
                var loaders = serviceProvider
                    .GetServices<IPluginCatalogConfigurationLoader>()
                    .ToList();

                var configuration = serviceProvider.GetService<IConfiguration>();

                var converters = serviceProvider.GetServices<IConfigurationToCatalogConverter>().ToList();
                var catalogs = new List<IPluginCatalog>();

                foreach (var loader in loaders)
                {
                    // Load the catalog configurations.
                    var catalogConfigs = loader.GetCatalogConfigurations(configuration);

                    if (catalogConfigs?.Any() != true)
                    {
                        continue;
                    }

                    for (var i = 0; i < catalogConfigs.Count; i++)
                    {
                        var item = catalogConfigs[i];
                        var key = $"{options.ConfigurationSection}:{loader.CatalogsKey}:{i}";

                        // Check if a type is provided.
                        if (string.IsNullOrWhiteSpace(item.Type))
                        {
                            throw new ArgumentException($"A type must be provided for catalog at position {i + 1}");
                        }

                        // Try to find any registered converter that can convert the specified type.
                        var foundConverter = converters.FirstOrDefault(converter => converter.CanConvert(item.Type));

                        if (foundConverter == null)
                        {
                            throw new ArgumentException($"The type provided for Plugin catalog at position {i + 1} is unknown.");
                        }

                        var catalog = foundConverter.Convert(configuration.GetSection(key));

                        catalogs.Add(catalog);
                    }
                }

                return new CompositePluginCatalog(catalogs.ToArray());
            });

            return services;
        }

        public static IServiceCollection AddPluginCatalog(this IServiceCollection services, IPluginCatalog pluginCatalog)
        {
            services.TryAddEnumerable(ServiceDescriptor.Singleton(typeof(IPluginCatalog), pluginCatalog));

            return services;
        }

        public static IServiceCollection AddPluginType<T>(this IServiceCollection services, ServiceLifetime serviceLifetime = ServiceLifetime.Transient,
            Action<DefaultPluginOption> configureDefault = null)
            where T : class
        {
            var serviceDescriptorEnumerable = new ServiceDescriptor(typeof(IEnumerable<T>), sp =>
            {
                var pluginProvider = sp.GetService<PluginProvider>();
                var result = pluginProvider.GetTypes<T>();

                return result.AsEnumerable();
            }, serviceLifetime);

            var serviceDescriptorSingle = new ServiceDescriptor(typeof(T), sp =>
            {
                var defaultPluginOption = GetDefaultPluginOptions<T>(configureDefault, sp);

                var pluginProvider = sp.GetService<PluginProvider>();
                var result = pluginProvider.GetTypes<T>();

                var defaultType = defaultPluginOption.DefaultType(sp, result.Select(r => r.GetType()));

                return result.Find(r => r.GetType() == defaultType);
            }, serviceLifetime);

            services.Add(serviceDescriptorEnumerable);
            services.Add(serviceDescriptorSingle);

            return services;
        }

        private static DefaultPluginOption GetDefaultPluginOptions<T>(Action<DefaultPluginOption> configureDefault, IServiceProvider sp) where T : class
        {
            var defaultPluginOption = new DefaultPluginOption();

            // If no configuration is provided though action try to get configuration from named options
            if (configureDefault == null)
            {
                var optionsFromMonitor =
                    sp.GetService<IOptionsMonitor<DefaultPluginOption>>().Get(typeof(T).Name);

                if (optionsFromMonitor != null)
                {
                    defaultPluginOption = optionsFromMonitor;
                }
            }
            else
            {
                configureDefault(defaultPluginOption);
            }

            return defaultPluginOption;
        }
    }
}
