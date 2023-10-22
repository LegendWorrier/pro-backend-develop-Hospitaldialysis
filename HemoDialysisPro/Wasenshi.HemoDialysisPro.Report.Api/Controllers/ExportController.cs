using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Telerik.Reporting.Services;
using Telerik.Reporting.Services.AspNetCore;

namespace Wasenshi.HemoDialysisPro.Report.Api.Controllers
{
    [AllowAnonymous]
    [Route("api/[controller]")]
    [ApiController]
    public class ExportController : ControllerBase
    {
        private readonly CoreReportResolver core;

        public ExportController(CoreReportResolver core)
        {
            this.core = core;
        }

        [AllowAnonymous]
        [HttpPost("pdf")]
        public async Task<IActionResult> GenerateReportPDF(ClientReportSource reportInfo)
        {
            var reportProcessor = new Telerik.Reporting.Processing.ReportProcessor();

            (var doc, var resolver) = core.GetReportAndResolver(reportInfo.Report);
            doc.DataSource = await resolver.PrepareData(reportInfo.ParameterValues);
            var source = core.GetReportSource(doc, reportInfo.ParameterValues);

            // set any deviceInfo settings if necessary
            var deviceInfo = new System.Collections.Hashtable();

            Telerik.Reporting.Processing.RenderingResult result = reportProcessor.RenderReport("PDF", source, deviceInfo);

            return File(result.DocumentBytes, "application/pdf", result.DocumentName);
        }
    }
}
