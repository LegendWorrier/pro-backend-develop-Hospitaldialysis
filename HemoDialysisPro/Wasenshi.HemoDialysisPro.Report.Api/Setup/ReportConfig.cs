using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Reflection;
using Telerik.Reporting.Cache.File;
using Telerik.Reporting.Services;

namespace Wasenshi.HemoDialysisPro.Report
{
    public static class ReportConfig
    {
        public static IServiceCollection AddReportSetting(this IServiceCollection services)
        {
            services.AddScoped<CoreReportResolver>();
            services.AddScoped<IReportSourceResolver, MainServedReportResolver>();

            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables()
                .AddInMemoryCollection().Build();
            config.GetSection("telerikReporting:assemblyReferences:0:name").Value =
                Assembly.GetAssembly(typeof(MainServedReportResolver)).GetName().Name;

            services.TryAddSingleton<IReportServiceConfiguration>(sp =>
                new ReportServiceConfiguration
                {
                    ReportingEngineConfiguration = config,
                    HostAppId = "HemoApp",
                    Storage = new FileStorage("report-caches"),
                    ReportSourceResolver = sp.CreateScope().ServiceProvider.GetService<IReportSourceResolver>(), // despite scope, this is singleton
                });

            services.AddAutoMapper((IServiceProvider s, IMapperConfigurationExpression c) =>
            {
                c.ConstructServicesUsing(t => s.GetService(t));
            }, typeof(ReportMapProfile));

            return services;
        }
    }
}
