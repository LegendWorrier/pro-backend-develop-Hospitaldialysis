using MessagePack;
using MessagePack.Resolvers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using Wasenshi.AuthPolicy;
using Wasenshi.AuthPolicy.Options;
using Wasenshi.HemoDialysisPro.Maps;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Models.Settings;
using Wasenshi.HemoDialysisPro.Repositories;
using Wasenshi.HemoDialysisPro.Services;
using Wasenshi.HemoDialysisPro.Services.Core.Mapper;
using Wasenshi.HemoDialysisPro.Services.Initialization;
using Wasenshi.HemoDialysisPro.Services.SmartLogin;
using Wasenshi.HemoDialysisPro.Share;
using Wasenshi.HemoDialysisPro.Models.ConfigExtesions;
using Wasenshi.HemoDialysisPro.Web.Api.AuthPolicy;
using Wasenshi.HemoDialysisPro.Web.Api.Setup;
using Wasenshi.HemoDialysisPro.Web.Api.Swagger;
using Wasenshi.HemoDialysisPro.Web.Api.Utils;
using Wasenshi.HemoDialysisPro.Web.Api.WebSocket;
using Zomp.EFCore.WindowFunctions.Npgsql;
using Role = Wasenshi.HemoDialysisPro.Models.Role;
using Wasenshi.HemoDialysisPro.Utils;
using Hangfire;
using Hangfire.Dashboard;
using ServiceStack.Messaging;
using StackExchange.Redis;
using Hangfire.Redis.StackExchange;

namespace Wasenshi.HemoDialysisPro.Web.Api
{
    public class Startup
    {
        private IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCustomCors();
            services.AddDbContextPool<ApplicationDbContext>(x => x.UseNpgsql(Configuration.GetConnectionString("HemodialysisConnection"),
                    c => c.UseWindowFunctions()
#if RELEASE
                   .EnableRetryOnFailure() //Default retry is 6 times according to the document (about 1 min total)
#endif
                )
            //.ConfigureWarnings(w => w.Throw(RelationalEventId.MultipleCollectionIncludeWarning))
            , 1024);

            services.AddOptions<OneTimeTokenProviderOptions>();

            services.AddIdentity<User, Role>(x => x.Password.RequireNonAlphanumeric = false)
                .AddErrorDescriber<LocalizationIdentityErrorDescriber>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders()
                .AddTokenProvider<SmartLoginTokenProvider>(SmartLogin.ID)
                .AddTokenProvider<OneTimeTokenProvider>(OneTimeToken.ID);
            services.AddOptions<DataProtectionTokenProviderOptions>()
                    .Configure(c =>
                    {
                        c.TokenLifespan = TimeSpan.FromDays(999999);
                    });

            services.AddLocalization(opt => opt.ResourcesPath = "Resources").AddMvc().AddViewLocalization(c => c.ResourcesPath = "Resources");
            services.Configure<RequestLocalizationOptions>(options =>
            {
                const string defaultCulture = "en-US";
                List<CultureInfo> supportedCultures = new()
                {
                    CultureInfo.GetCultureInfo(defaultCulture),
                    CultureInfo.GetCultureInfo(Configuration["CULTURE"]) ?? CultureInfo.CreateSpecificCulture(Configuration["CULTURE"])
                };

                options.DefaultRequestCulture = new RequestCulture(defaultCulture);
                options.SupportedCultures = supportedCultures;
                options.SupportedUICultures = supportedCultures;

                ((CookieRequestCultureProvider)options.RequestCultureProviders[1]).CookieName = "Lang";
            });

            services.AddValidation();
            services.AddScopes(Configuration);

            services.AddControllers().AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new DateTimeConverter()));
            services.AddControllersWithViews();
            services.AddHttpHelper();
            services.AddHttpClient();
            services.AddHealthChecks();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "HemoDialysisPro API",
                    Description = "An Api for HemoDialysisPro operations",
                    Contact = new OpenApiContact
                    {
                        Name = "SoftTech Medcare Co.,Ltd. (In collaboration with Nikkiso Medical Thailand Co.,Ltd.)",
                        Email = "support@softtechmedcare.com"
                    }
                });
                c.Setup();

                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            });

            services.AddAuthenConfig(Configuration);
            services.AddHemoAuthConfig(Configuration);
            services.AddPolicy(o =>
            {
                o.RegisterUserConfig<UserConfig, Guid>()
                .AddRoleLevelPolicy<Guid>()
                .AddRoleLevelPolicy<Guid>("Delete", c => c.AllowSelf = false)
                .RoleLevelTableSetting
                    .SetRole(Roles.PowerAdmin, 5)
                    .SetRole(Roles.Admin, 4)
                    .SetRole(Roles.Doctor, 3)
                    .SetRole(Roles.HeadNurse, 2)
                    .SetRole(Roles.Nurse, 1)
                    .SetRole(Roles.PN, 0);
                o.GlobalPermission = Permissions.GLOBAL;
            });
            services.AddMappingConfig<ApplicationDbContext>(x => x.AddMaps(typeof(HemoProfile)));
            services.AddLicenseProtect();
            var unitSetting = Configuration.GetSection("UnitSettings");
            services.ConfigureWritable<UnitSettings>(unitSetting);
            var globalSetting = Configuration.GetSection("GlobalSettings");
            services.ConfigureWritable<GlobalSetting>(globalSetting);

            services.AddSignalR(c =>
                {
                    c.KeepAliveInterval = TimeSpan.FromSeconds(7.5);
                    // c.EnableDetailedErrors = true; // use this only on Development
                })
                .AddHubOptions<HemoBoxHub>(c =>
                {
                    c.ClientTimeoutInterval = TimeSpan.FromSeconds(10);
                    c.KeepAliveInterval = TimeSpan.FromSeconds(5);
                })
            .AddMessagePackProtocol(o =>
                {
                    o.SerializerOptions = MessagePackSerializerOptions
                        .Standard
                        .WithResolver(CompositeResolver.Create(
                            BuiltinResolver.Instance,
                            DynamicGenericResolver.Instance,
                            DynamicUnionResolver.Instance,
                            ContractlessStandardResolver.Instance,
                            PrimitiveObjectResolver.Instance))
                        .WithSecurity(MessagePackSecurity.UntrustedData);
                });

#if !TEST
            ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(Configuration["RedisConnection"]);
            services.AddHangfire(c => c
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseRedisStorage(redis)
                .UseSerilogLogProvider());

            services.AddRedisAndMQ(Configuration);

            services.AddBackgroundTasks();
#endif
            services.ExtraSetup();
            services.ConfigPluginSystem();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "HemoDialysisPro API V1");
                    c.RoutePrefix = string.Empty;
                });
            }

            app.InitializeData();

            app.UseRequestLocalization();

            app.UseLogging();
            app.UseRouting();
            app.UseCors();

            app.UseMiddleware<ErrorHandlerMiddleware>();
            app.UseWebSockets();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseMiddleware<APIBlockMiddleware>();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHealthChecks("/health").RequireAuthorization("bypass");
                endpoints.MapGet("/available", APIBlockMiddleware.CheckAvailibility).RequireAuthorization("bypass");
                endpoints.MapHub<HemoBoxHub>("/box").RequireAuthorization("bypass");
                endpoints.MapHub<CheckInHub>("/checkin").RequireAuthorization("bypass");
                endpoints.MapHub<UserHub>("/connect"); // for real-time update, notification, and monitoring
#if !TEST
                endpoints.MapHangfireDashboardWithAuthorizationPolicy("backend-admin", "/jobs", new DashboardOptions
                {
                    DashboardTitle = "HemoPro Background Tasks",
                    Authorization = new List<IDashboardAuthorizationFilter>(),
                });
                endpoints.MapDefaultControllerRoute();
#endif
            });

            // start listening to message queue and global events
#if !TEST
            var mq = app.ApplicationServices.GetService<IMessageService>();
            mq.Start();
#endif
        }
    }
}
