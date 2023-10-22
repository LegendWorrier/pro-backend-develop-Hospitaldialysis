using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using Telerik.Reporting;
using Telerik.Reporting.Services;
using Wasenshi.HemoDialysisPro.Report.DocumentLogics;

namespace Wasenshi.HemoDialysisPro.Report
{
    public class MainServedReportResolver : IReportSourceResolver
    {

        private readonly ILoggerFactory logger;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IDistributedCache distributedCache;
        private readonly CoreReportResolver core;

        public MainServedReportResolver(
            ILoggerFactory logger,
            IHttpContextAccessor httpContextAccessor,
            IDistributedCache distributedCache,
            CoreReportResolver core)
        {
            this.logger = logger;
            this.httpContextAccessor = httpContextAccessor;
            this.distributedCache = distributedCache;
            this.core = core;
        }

        public ReportSource Resolve(string report, OperationOrigin operationOrigin, IDictionary<string, object> currentParameterValues)
        {
            try
            {
                (var doc, IDocResolver resolver) = core.GetReportAndResolver(report);

                var context = httpContextAccessor.HttpContext;
                var path = context.Request.Path.Value;
                var sessionId = path.Split('/')[4];
                if (operationOrigin == OperationOrigin.ResolveReportParameters)
                {
                    object data;
                    if (currentParameterValues.Count > 0 && (currentParameterValues.ContainsKey("userId") || !currentParameterValues.ContainsKey("locale")))
                    {
                        // Prepare Data
                        data = resolver.PrepareData(currentParameterValues).GetAwaiter().GetResult();
                    }
                    else
                    {
                        // Update Data
                        var prevData = distributedCache.Get(sessionId)?.ByteArrayToObject();
                        data = resolver.UpdateData(prevData).GetAwaiter().GetResult();
                        if (data == null)
                        {
                            return null;
                        }
                    }

                    doc.DataSource = data;

                    var options = new DistributedCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(10));
                    distributedCache.Set(sessionId, data.ObjectToByteArray(), options);
                }
                else
                {
                    doc.DataSource = resolver.GetData(distributedCache.Get(sessionId)?.ByteArrayToObject());
                }

                var source = core.GetReportSource(doc, currentParameterValues);

                // extra
                resolver.ExtraSetup(source);

                return source;
            }
            catch (Exception e)
            {
                logger.CreateLogger<MainServedReportResolver>().LogError("Report Resolving failed: {Message} || {Exception}", e.Message, e);
                throw;
            }
        }
    }
}
