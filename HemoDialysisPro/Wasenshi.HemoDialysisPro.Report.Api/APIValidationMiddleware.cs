using DocumentFormat.OpenXml.InkML;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ServiceStack;
using ServiceStack.Redis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Wasenshi.AuthPolicy;
using Wasenshi.AuthPolicy.Utillities;
using Wasenshi.HemoDialysisPro.Repositories.Interfaces;
using Wasenshi.HemoDialysisPro.Services.Interfaces;
using Wasenshi.HemoDialysisPro.Share;
using Wasenshi.HemoDialysisPro.Utils;

namespace Wasenshi.HemoDialysisPro.Report.Api
{
    public class APIValidationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<APIValidationMiddleware> logger;
        private readonly IServiceProvider service;

        /// <summary>
        /// For improving performance and reduce DB round trip
        /// </summary>
        public readonly ConcurrentHashSet<string> cache = new ConcurrentHashSet<string>();

        public APIValidationMiddleware(RequestDelegate next, ILogger<APIValidationMiddleware> logger, IServiceProvider service)
        {
            _next = next;
            this.logger = logger;
            this.service = service;
        }

        public async Task Invoke(HttpContext context)
        {
            // check doctor
            using var scope = service.CreateScope();
            var redis = scope.ServiceProvider.GetRequiredService<IRedisClient>();
            bool seeOwnPatientOnly = redis.Get<bool>(Common.SEE_OWN_PATIENT_ONLY);
            if (IsReportPreparationRequest(context) &&
                seeOwnPatientOnly && context.User.IsInRole(Roles.Doctor) && !context.User.IsInRole(Roles.PowerAdmin))
            {
                var bodyText = await context.Request.Body.ReadToEndAsync();
                context.Request.Body.Position = 0; // reset back header also
                var body = JsonSerializer.Deserialize<ReportRequest>(bodyText, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                });

                var doctorId = context.User.GetUserIdAsGuid();
                if (body.Report == "hemosheet")
                {
                    body.ParameterValues.TryGetValue("hemoId", out string hemoId);
                    //var cacheStr = $"{context.User.GetUserId()}:{hemoId}";
                    if (!string.IsNullOrEmpty(hemoId))
                    {
                        var hemoRepo = scope.ServiceProvider.GetRequiredService<IHemoRecordRepository>();
                        var hemosheet = hemoRepo.Get(new Guid(hemoId));
                        var patientRepository = scope.ServiceProvider.GetRequiredService<IPatientRepository>();
                        var patient = patientRepository.Get(hemosheet.PatientId);
                        if (!patient.DoctorId.HasValue || patient.DoctorId != doctorId)
                        {
                            throw new AppException("UNAUTHORIZED", "Cannot access the patient.");
                        }
                        //cache.Add(cacheStr);
                    }
                }
                else if (body.Report == "hemorecords")
                {
                    body.ParameterValues.TryGetValue("patientId", out string patientId);
                    //var cacheStr = $"{context.User.GetUserId()}:{patientId}";
                    if (!string.IsNullOrEmpty(patientId))
                    {
                        var patientRepository = scope.ServiceProvider.GetRequiredService<IPatientRepository>();
                        var patient = patientRepository.Get(patientId);
                        if (!patient.DoctorId.HasValue || patient.DoctorId != doctorId)
                        {
                            throw new AppException("UNAUTHORIZED", "Cannot access the patient.");
                        }

                        //cache.Add(cacheStr);
                    }
                }

                
            }

            await _next(context);
        }

        private static bool IsReportPreparationRequest(HttpContext context)
        {
            return context.Request.Path.HasValue && (context.Request.Path.Value.EndsWithIgnoreCase("parameters"));
        }

        struct ReportRequest
        {
            public Dictionary<string, string> ParameterValues { get; set; }
            public string Report { get; set; }
        }
    }
}
