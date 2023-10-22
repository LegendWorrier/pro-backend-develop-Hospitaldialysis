using FluentHttpClient;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using ServiceStack.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Wasenshi.AuthPolicy;
using Wasenshi.AuthPolicy.Attributes;
using Wasenshi.HemoDialysisPro.Models.Settings;
using Wasenshi.HemoDialysisPro.Services.Interfaces;
using Wasenshi.HemoDialysisPro.Share;

namespace Wasenshi.HemoDialysisPro.Web.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConfigController : ControllerBase
    {
        private readonly IConfiguration config;
        private readonly IWebHostEnvironment webHost;
        private readonly IUploadService uploadService;
        private readonly IRedisClient redis;
        private readonly IHttpClientFactory clientFactory;

        public ConfigController(IConfiguration config, IWebHostEnvironment webHost, IUploadService uploadService, IRedisClient redis, IHttpClientFactory clientFactory)
        {
            this.config = config;
            this.webHost = webHost;
            this.uploadService = uploadService;
            this.redis = redis;
            this.clientFactory = clientFactory;
        }

        [PermissionAuthorize(Permissions.CONFIG)]
        [HttpPost]
        public async Task<IActionResult> SetConfig([FromForm] IFormCollection data)
        {
            // upload logo
            if (data.Files.Any())
            {
                await this.ValidatePermissionAsync(Permissions.BASIC);
                var logo = data.Files["logo"] ?? throw new InvalidOperationException("Key is wrong. No logo has been uploaded.");
                await uploadService.Upload(uploadService.ResizeImage(logo, 2400, 650), "logo");
            }

            // config setting
            List<EditAppConfig> appConfigs = new(data.Select(x => new EditAppConfig { Name = x.Key, Value = x.Value }));
            if (IsBasicConfig(appConfigs))
            {
                await this.ValidatePermissionAsync(Permissions.BASIC);
            }
            else if (IsDialysisRecordConfig(appConfigs))
            {
                await this.ValidatePermissionAsync(Permissions.DIALYSIS);
            }
            else if (IsDialysisPrescriptionConfig(appConfigs))
            {
                await this.ValidatePermissionAsync(Permissions.PRESCRIPTION);
            }
            else if (IsPatientConfig(appConfigs))
            {
                await this.ValidatePermissionAsync(Permissions.Patient.SETTING);
            }
            else if (IsLabExamConfig(appConfigs))
            {
                await this.ValidatePermissionAsync(Permissions.LABEXAM);
            }

            var secureDomain = data["secureDomain"];
            if (secureDomain != StringValues.Empty)
            {
                if (!data.ContainsKey("apiServer"))
                {
                    var uriType = Uri.CheckHostName(secureDomain.ToString());
                    switch (uriType)
                    {
                        case UriHostNameType.IPv4:
                        case UriHostNameType.IPv6:
                            appConfigs.Add(new EditAppConfig { Name = "apiServer", Value = secureDomain });
                            break;
                        default:
                            appConfigs.Add(new EditAppConfig { Name = "apiServer", Value = $"https://backend.{secureDomain}" });
                            break;
                    }
                }

                if (!data.ContainsKey("reportService"))
                {
                    appConfigs.Add(new EditAppConfig { Name = "reportService", Value = appConfigs.Find(x => x.Name == "apiServer").Value });
                }
            }

            // global settings
            foreach (var item in appConfigs.Where(x => x.Name.StartsWith("global:")))
            {
                string key = item.Name[7..]; // cut 'global:' part out
                switch (char.ToUpper(key[0]) + key[1..])
                {
                    case nameof(GlobalSetting.LogoAlign):
                        if (!int.TryParse(item.Value, out int align))
                        {
                            throw new InvalidCastException($"Cannot parse value for '{nameof(GlobalSetting.LogoAlign)}'. [{item.Value}]");
                        }
                        redis.Set(Common.LOGO_ALIGN, (Align)align);

                        break;
                    default:
                        break;
                }
            }

            var client = clientFactory.CreateClient();
            var result = await client.PostAsJsonAsync(config["ConfigApiUrl"], appConfigs);

            if (!result.IsSuccessStatusCode)
            {
                var response = await result.GetResponseStringAsync();
                return StatusCode(500, response);
            }

            return Ok();
        }

        private static bool IsBasicConfig(List<EditAppConfig> appConfigs)
        {
            bool result = appConfigs.Any(x => x.Name.StartsWith("global:"))
                            || appConfigs.Any(x => x.Name == "centerName")
                            || appConfigs.Any(x => x.Name == "centerType")
                            || appConfigs.Any(x => x.Name == "decimalPrecision")
                            || appConfigs.Any(x => x.Name == "his")
                            || appConfigs.Any(x => x.Name == "secureDomain")
                            || appConfigs.Any(x => x.Name == "apiServer")
                            || appConfigs.Any(x => x.Name == "reportService");

            return result;
        }

        private static bool IsDialysisRecordConfig(List<EditAppConfig> appConfigs)
        {
            return appConfigs.Any(x => x.Name == "dialysisRecord");
        }

        private static bool IsDialysisPrescriptionConfig(List<EditAppConfig> appConfigs)
        {
            return appConfigs.Any(x => x.Name == "prescription");
        }

        private static bool IsPatientConfig(List<EditAppConfig> appConfigs)
        {
            return appConfigs.Any(x => x.Name == "patient");
        }

        private static bool IsLabExamConfig(List<EditAppConfig> appConfigs)
        {
            return appConfigs.Any(x => x.Name == "labExam");
        }

        public class EditAppConfig
        {
            public string Name { get; set; }
            public string Value { get; set; }
        }
    }
}
