using Microsoft.AspNetCore.Mvc;
using System;
using Telerik.Reporting.Services;
using Telerik.Reporting.Services.AspNetCore;

namespace Wasenshi.HemoDialysisPro.Report.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportController : ReportsControllerBase
    {
        public ReportController(IReportServiceConfiguration reportServiceConfiguration) : base(reportServiceConfiguration)
        {
        }
    }
}
