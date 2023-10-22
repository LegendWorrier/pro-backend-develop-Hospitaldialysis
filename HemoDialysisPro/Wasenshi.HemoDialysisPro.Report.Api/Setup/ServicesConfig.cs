using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Wasenshi.HemoDialysisPro.Report.DocumentLogics;
using Wasenshi.HemoDialysisPro.Report.Models;
using Wasenshi.HemoDialysisPro.Repositories;
using Wasenshi.HemoDialysisPro.Services.Core;
using Wasenshi.HemoDialysisPro.Services.Core.Interfaces;

namespace Wasenshi.HemoDialysisPro.Report.Api
{
    public static class ServicesConfig
    {
        public static IServiceCollection AddScopes(this IServiceCollection services, IConfiguration configuration)
        {
            services.RegisterRepositories();
            services.AddScoped<IUserResolver, UserResolver>()
                    .AddScoped<IUserInfoService, UserInfoService>()
                    // docs resolvers
                    .AddScoped<HemosheetResolver>()
                    .AddScoped<HemoAdequacyResolver>()
                    // data
                    .AddTransient<NurseRecordData>()
                    .AddTransient<DoctorRecordData>()
                    .AddTransient<ProgressNoteData>()
                    .AddTransient<MedicineRecordData>();
            

            return services;
        }
    }
}
