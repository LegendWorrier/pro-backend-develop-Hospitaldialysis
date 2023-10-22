using FluentHttpClient;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Wasenshi.HemoDialysisPro.PluginBase;
using Wasenshi.HemoDialysisPro.PluginIntegrate;
using Wasenshi.HemoDialysisPro.Services.Interfaces;

namespace Wasenshi.HemoDialysisPro.Web.Api.Controllers.Utils
{
    [Route("api/[controller]")]
    [AllowAnonymous]
    [ApiController]
    public class UtilsController : ControllerBase
    {
        private readonly IHttpClientFactory clientFactory;
        private readonly IHemoService hemoService;
        private readonly IServiceProvider sp;
        private readonly IEnumerable<IDocumentHandler> docPlugins;

        public UtilsController(IHttpClientFactory clientFactory, IHemoService hemoService, IServiceProvider sp, IEnumerable<IDocumentHandler> docPlugins)
        {
            this.clientFactory = clientFactory;
            this.hemoService = hemoService;
            this.sp = sp;
            this.docPlugins = docPlugins;
        }

        [RequestSizeLimit(25000000)] // 25MB
        [HttpPost("zip-native-file")]
        public IActionResult ZipNativeFile([FromForm] IFormFile nativeFile, [FromForm] string jsonConfigFile, [FromForm] string configPath = "assets/config/config.json")
        {
            byte[] file = null;
            string extension = nativeFile.FileName.Split('.')[1];

            using (var m = new MemoryStream())
            {
                nativeFile.OpenReadStream().CopyTo(m);

                if (extension == "apk")
                {
                    const string nativePath = "assets/public";
                    using (ZipArchive archive = new ZipArchive(m, ZipArchiveMode.Update, true))
                    {
                        string path = Path.Combine(nativePath, configPath);
                        ZipArchiveEntry entry = archive.GetEntry(path);
                        if (entry != null)
                        {
                            entry.Delete();
                        }
                        entry = archive.CreateEntry(Path.Combine(nativePath, configPath), CompressionLevel.Optimal);
                        using (StreamWriter writer = new StreamWriter(entry.Open()))
                        {
                            writer.Write(jsonConfigFile);
                        }
                    }
                }
                else if (extension == "ipa")
                {
                    const string nativePath = "notsure";
                    //TODO: ios part
                }
                else
                {
                    return BadRequest();
                }

                file = m.ToArray();
            }

            return File(file, $"application/{extension}", nativeFile.FileName);
        }

        /// <summary>
        /// This is used only for testing.
        /// </summary>
        /// <returns></returns>
        [HttpPost("pdf")]
        public async Task<IActionResult> GenerateReportPDF()
        {
            var hemoResult = hemoService.GetAllHemodialysisRecords(1, 1);
            if (hemoResult.Total > 0)
            {
                var hemo = hemoResult.Data.First();
                var hemoId = hemo.Record.Id;
                docPlugins.ExecutePluginsOnBackgroundThread(async (docHandler, sp) =>
                {
                    var logger = sp.GetService<ILoggerFactory>().CreateLogger("PDF Plugin");
                    try
                    {
                        await docHandler.SendHemosheet(hemo.Record);
                    }
                    catch (Exception e)
                    {
                        logger.LogError(e, "Plugin error at util controller thread.");
                    }
                    
                });

                return Ok();
            }

            return NotFound();
        }
    }
}
