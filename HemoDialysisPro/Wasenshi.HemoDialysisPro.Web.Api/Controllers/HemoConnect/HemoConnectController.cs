using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Wasenshi.HemoDialysisPro.Services.Interfaces;
using Wasenshi.HemoDialysisPro.Share;

namespace Wasenshi.HemoDialysisPro.Web.Api.Controllers
{
    [AllowAnonymous]
    [Route("api/[controller]")]
    [ApiController]
    public class HemoConnectController : ControllerBase
    {
        private readonly IConfiguration config;
        private readonly IWebHostEnvironment webHost;
        private readonly IUploadService uploadService;

        public readonly string hemoConnectPath = Path.Combine(
#if DEBUG
            AppDomain.CurrentDomain.BaseDirectory

#else
            Environment.CurrentDirectory
#endif
            , "apps", "Connect");

        public HemoConnectController(IConfiguration config, IWebHostEnvironment webHost, IUploadService uploadService)
        {
            this.config = config;
            this.webHost = webHost;
            this.uploadService = uploadService;
        }

        [HttpGet("download")]
        public async Task<IActionResult> Download()
        {

            var fileList = Directory.EnumerateFiles(hemoConnectPath)
                    .OrderByDescending(x => GetFileVersionFromName(x) ?? GetFileVersion(x))
                    .ThenByDescending(x => new FileInfo(Path.Combine(hemoConnectPath, x)).LastWriteTimeUtc)
                    .ToList();

            if (fileList.Count == 0)
            {
                return NotFound();
            }

            string latest = fileList[0];

            var fileStream = System.IO.File.OpenRead(latest);

            return File(fileStream, "application/vnd.microsoft.portable-executable", "HemoConnectInstaller.exe");
        }

        private Version GetFileVersionFromName(string filePath)
        {
            var fileInfo = new FileInfo(filePath);
            var splits = Path.GetFileNameWithoutExtension(fileInfo.Name).Split('-');
            var latestVersion = splits.Length > 1 ? splits.Last().Replace(".tar", "").Replace(".gz", "").Replace(".exe", "") : null;

            if (latestVersion != null)
            {
                return Version.Parse(latestVersion);
            }

            return null;
        }

        private Version GetFileVersion(string filePath)
        {
            if (new FileInfo(filePath).Extension != ".exe")
            {
                return null;
            }
            var fileInfo = FileVersionInfo.GetVersionInfo(filePath);
            string versionStr = fileInfo.ProductVersion ?? fileInfo.FileVersion;
            if (versionStr == null)
            {
                return null;
            }
            return Version.Parse(versionStr);
        }
    }
}
