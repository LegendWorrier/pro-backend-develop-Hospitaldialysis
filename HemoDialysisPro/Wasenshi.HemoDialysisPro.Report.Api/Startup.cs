using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using System;
using System.IO;
using System.Reflection;
using Wasenshi.HemoDialysisPro.Report.Api.Setup;
using Wasenshi.HemoDialysisPro.Report.Api.Swagger;
using Wasenshi.HemoDialysisPro.Repositories;
using Wasenshi.HemoDialysisPro.Utils;
using Wasenshi.HemoDialysisPro.Services;
using Microsoft.AspNetCore.Localization;
using System.Collections.Generic;
using System.Globalization;

namespace Wasenshi.HemoDialysisPro.Report.Api
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
            services.AddDbContextPool<ApplicationDbContext>(x => x.UseNpgsql(Configuration.GetConnectionString("HemodialysisConnection")
#if RELEASE
                   , c => c.EnableRetryOnFailure() //Default retry is 6 times according to the document (about 1 min total)
#endif
            )
            //.ConfigureWarnings(w => w.Throw(RelationalEventId.MultipleCollectionIncludeWarning))
            , 1024);

            services.Configure<RequestLocalizationOptions>(options =>
            {
                const string defaultCulture = "en-US";
                List<CultureInfo> supportedCultures = new()
                {
                    CultureInfo.GetCultureInfo(defaultCulture),
                    CultureInfo.GetCultureInfo(Configuration["CULTURE"])
                };

                options.DefaultRequestCulture = new RequestCulture(defaultCulture);
                options.SupportedCultures = supportedCultures;
                options.SupportedUICultures = supportedCultures;

                ((CookieRequestCultureProvider)options.RequestCultureProviders[1]).CookieName = "Lang";
            });

            services.AddScopes(Configuration);

            services.AddControllers().AddNewtonsoftJson();
            services.AddHttpContextAccessor();
            services.AddHealthChecks();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "HemoDialysisPro Report API",
                    Description = "An Api for HemoDialysisPro Reports",
                    Contact = new OpenApiContact
                    {
                        Name = "Wasenshi",
                        Email = "recka123@gmail.com"
                    }
                });
                c.DocumentFilter<SecurityRequirementsDocumentFilter>();
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme { In = ParameterLocation.Header, Description = "Please insert JWT with Bearer into field", Name = "Authorization", Type = SecuritySchemeType.ApiKey });

                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
                c.IgnoreObsoleteActions();
            });

            services.AddAuthenConfig(Configuration);
            services.AddReportSetting();
            services.AddRedis(Configuration);

            var cacheStrategy = Configuration["Reports:CacheStrategy"];
            if (string.IsNullOrWhiteSpace(cacheStrategy) || string.Equals(cacheStrategy, "inmemory", StringComparison.OrdinalIgnoreCase))
            {
                services.AddDistributedMemoryCache();
            }
            else if (cacheStrategy == "redis")
            {
                services.AddStackExchangeRedisCache(c =>
                {
                    c.InstanceName = Configuration["Reports:Cache:Prefix"];
                    c.Configuration = Configuration["Reports:Cache:RedisConfiguration"];
                });
            }

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
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "HemoDialysisPro Report V1");
                    c.RoutePrefix = string.Empty;
                });
            }

            app.UseRequestLocalization();

            app.UseLogging();
            app.UseRouting();
            app.UseCors();

            app.UseMiddleware<ErrorHandlerMiddleware>();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseMiddleware<APIValidationMiddleware>();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHealthChecks("/health").RequireAuthorization("bypass");
            });
        }
    }
}
