using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;
using Wasenshi.HemoDialysisPro.Services.Interfaces;
using Wasenshi.HemoDialysisPro.Share;

namespace Wasenshi.HemoDialysisPro.Web.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UploadController : ControllerBase
    {
        private readonly IConfiguration config;
        private readonly IWebHostEnvironment webHost;
        private readonly IUploadService uploadService;

        public UploadController(IConfiguration config, IWebHostEnvironment webHost, IUploadService uploadService)
        {
            this.config = config;
            this.webHost = webHost;
            this.uploadService = uploadService;
        }

        [Authorize(Roles = Roles.Admin)]
        [HttpPost]
        public async Task<IActionResult> UploadFile([FromForm] IFormCollection data)
        {
            List<string> IDs = new List<string>();
            foreach (var item in data.Files)
            {
                var uri = await uploadService.Upload(item, item.Name);
                IDs.Add(uri);
            }

            return Ok(IDs);
        }

        [HttpGet("{fileId}")]
        public async Task<IActionResult> GetFile(string fileId)
        {
            var file = await uploadService.Get(fileId);

            if (file.bytes == null)
            {
                return NotFound();
            }

            return File(file.bytes, file.contentType);
        }
    }
}
