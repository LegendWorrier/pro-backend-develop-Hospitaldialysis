using FluentHttpClient;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Models.TimezoneUtil;
using Wasenshi.HemoDialysisPro.PluginBase;
using Wasenshi.HemoDialysisPro.Repositories.Interfaces;
using Wasenshi.HemoDialysisPro.Repositories.Interfaces.Base;
using Wasenshi.HemoDialysisPro.Services.Interfaces;

namespace Wasenshi.HemoDialysisPro.Services.Core
{
    public class HemoPro : IHemoPro
    {
        private readonly IHttpClientFactory clientFactory;
        private readonly IServiceProvider provider;

        private readonly string exportPdfUrl =
#if DEBUG
    "http://host.docker.internal:8400/api/export/pdf";
#else
    "http://hemoreport/api/export/pdf";
#endif

        public IConfiguration Configuration { get; }

        public TimeZoneInfo TimeZone { get; }

        public HemoPro(IConfiguration config, IHttpClientFactory clientFactory, IServiceProvider provider)
        {
            Configuration = config;
            this.clientFactory = clientFactory;
            this.provider = provider;
            TimeZone = TimezoneUtils.GetTimeZone(Configuration["TIMEZONE"]);
        }

        public async Task<byte[]> GenerateHemoAdequacyPdf(string patientId, DateOnly month)
        {
            const string cookieName = "Lang";
            var cookieValue = CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(CultureInfo.CurrentUICulture));
            var client = clientFactory.CreateClient();
            return await client.UsingRoute(exportPdfUrl)
                .WithCookie(cookieName, cookieValue)
                .WithJsonContent(new
                {
                    Report = "hemorecord",
                    ParameterValues = new Dictionary<string, string>
                    {
                        { "patientId", patientId },
                        { "month", month.ToShortDateString() }
                    }
                })
                .PostAsync()
                .GetResponseBytesAsync();
        }

        public async Task<byte[]> GenerateHemosheetPdf(Guid hemosheetId)
        {
            const string cookieName = "Lang";
            var cookieValue = CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(CultureInfo.CurrentUICulture));
            var client = clientFactory.CreateClient();
            return await client.UsingRoute(exportPdfUrl)
                .WithCookie(cookieName, cookieValue)
                .WithJsonContent(new
                {
                    Report = "hemosheet",
                    ParameterValues = new Dictionary<string, string>
                    {
                        { "hemoId", hemosheetId.ToString() }
                    }
                })
                .PostAsync()
                .GetResponseBytesAsync();
        }

        public Task MarkHemosheetAsSent(Guid hemosheetId)
        {
            var hemoService = provider.GetRequiredService<IHemoService>();
            var hemosheet = hemoService.GetHemodialysisRecord(hemosheetId);
            hemosheet.SentPDF = true;
            hemoService.EditHemodialysisRecord(hemosheet);

            return Task.CompletedTask;
        }



        public Patient GetPatient(string patientId)
        {
            var patientService = provider.GetService<IPatientService>();
            if (patientService == null)
            {
                var patientRepo = provider.GetService<IPatientRepository>();
                return patientRepo.Get(patientId);
            }
            return patientService.GetPatient(patientId);
        }

        public Unit GetUnit(int unitId)
        {
            var unitRepo = provider.GetService<IRepository<Unit, int>>();

            return unitRepo.GetAll(false).FirstOrDefault(x => x.Id == unitId);
        }

        public T Resolve<T>()
        {
            var result = (T)ActivatorUtilities.CreateInstance(provider, typeof(T));
            return result;
        }

        
    }
}
