using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Wasenshi.HemoDialysisPro.PluginBase;
using Wasenshi.HemoDialysisPro.PluginBase.Stats;
using Wasenshi.HemoDialysisPro.PluginIntegrate.IntegrateHelperWithoutAsp;
using Weikio.PluginFramework.Abstractions;
using Weikio.PluginFramework.Catalogs;
using Weikio.PluginFramework.Context;
using Weikio.PluginFramework.TypeFinding;

namespace Wasenshi.HemoDialysisPro.PluginIntegrate
{
    public static class PluginConfiguration
    {
        public static IServiceCollection ConfigPluginSystem<THemoPro, TStatProcessor, TStatInfoProcessor>(this IServiceCollection services)
            where THemoPro : class, IHemoPro
            where TStatProcessor : class, IStatProcessor
            where TStatInfoProcessor : class, IStatInfoProcessor
        {
#if !DEBUG
            PluginLoadContextOptions.Defaults.UseHostApplicationAssemblies = UseHostApplicationAssembliesEnum.PreferPlugin;
#endif

            PluginLoadContextOptions.Defaults.AdditionalRuntimePaths.Add(@"plugins");

            TypeFinderOptions.Defaults.TypeFinderCriterias = new List<TypeFinderCriteria>
            {
                TypeFinderCriteriaBuilder.Create().Implements<IAuthHandler>().Tag("auth"),
                TypeFinderCriteriaBuilder.Create().Implements<IDocumentHandler>().Tag("doc"),
                TypeFinderCriteriaBuilder.Create().Implements<IStatHandler>().Tag("stat")
            };
            var folderPluginCatalog = new FolderPluginCatalog(@"plugins", new FolderPluginCatalogOptions
            {
                IncludeSubfolders = true
            });

            services.AddHttpClient();
            services.AddTransient<IHemoPro, THemoPro>();
            services.AddTransient<IStatProcessor, TStatProcessor>();
            services.AddTransient<IStatInfoProcessor, TStatInfoProcessor>();
            services.AddPluginFrameworkCore()
                .AddPluginCatalog(folderPluginCatalog)
                .AddPluginType<IAuthHandler>()
                .AddPluginType<IDocumentHandler>()
                .AddPluginType<IStatHandler>();

            return services;
        }
    }
}