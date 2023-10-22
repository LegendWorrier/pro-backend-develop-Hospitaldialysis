// See https://aka.ms/new-console-template for more information

using AutoMapper;
using Hangfire;
using Hangfire.Redis.StackExchange;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using StackExchange.Redis;
using Wasenshi.HemoDialysisPro.Jobs;
using Wasenshi.HemoDialysisPro.JobServer;
using Wasenshi.HemoDialysisPro.JobsServer;
using Wasenshi.HemoDialysisPro.PluginIntegrate;
using Wasenshi.HemoDialysisPro.Services;
using Wasenshi.HemoDialysisPro.Services.Core.Mapper;
using Wasenshi.HemoDialysisPro.Share;

var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", false, true)
    .AddEnvironmentVariables()
    .Build();

const string DEFAULT_LOCAL_SEQ = "http://localhost:5341";
Log.Logger = new LoggerConfiguration()
#if DEBUG
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Debug)
#else
                .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
#endif
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                .MinimumLevel.Override("Hangfire", LogEventLevel.Warning)
                .WriteTo.Seq(Environment.GetEnvironmentVariable("SEQ_URL") ?? DEFAULT_LOCAL_SEQ)
                .WriteTo.Console()
                .CreateLogger();

var hostBuilder = Host.CreateDefaultBuilder(args);
hostBuilder.ConfigureServices(hostServices =>
{
    hostServices
        .AddLogging(c => c.AddSerilog())
        .RegisterRedis(config)
        .AddAutoMapper(typeof(HemoProfile))
        .RegisterJobsAndDependencies<ContextAdapterJobServer, UserAdapterJobServer>(config)
        .AddDbContextPool<ApplicationDbContextJobServer>(x => x.UseNpgsql(config.GetConnectionString("HemodialysisConnection")
#if RELEASE
                   , c => c.EnableRetryOnFailure() //Default retry is 6 times according to the document (about 1 min total)
#endif
                ), 1024)
        .AddLicenseProtectWithLog()
        .ConfigPluginSystem();

    // ===== Use hosted services for hangfire background job server =========
    var serviceProvider = hostServices.BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = true, ValidateScopes = true });

    var adapter = new ServiceProviderAdapter(serviceProvider);

    ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(config["RedisConnection"]);
    GlobalConfiguration.Configuration
        .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
        .UseSimpleAssemblyNameTypeSerializer()
        .UseRecommendedSerializerSettings()
        .UseRedisStorage(redis)
        .UseActivator(adapter);
    hostServices.AddHostedService<HostedBgJobServer>(); // Hangfire Server
});
hostBuilder.UseSerilog();

await hostBuilder.RunConsoleAsync();
