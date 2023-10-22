using DocumentFormat.OpenXml.Bibliography;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telerik.Reporting;
using Wasenshi.HemoDialysisPro.Report.DocumentLogics;

namespace Wasenshi.HemoDialysisPro.Report
{
    public class CoreReportResolver
    {
        public static readonly string BasePath = Path.Combine(
#if DEBUG
            AppDomain.CurrentDomain.BaseDirectory

#else
            Environment.CurrentDirectory
#endif
           , "Reports");
        public static readonly string uploadsFolder = Path.Combine(
#if DEBUG
            AppDomain.CurrentDomain.BaseDirectory

#else
            Environment.CurrentDirectory
#endif
            , "upload");
        private readonly IConfiguration config;
        private readonly HemosheetResolver hemosheetResolver;
        private readonly HemoAdequacyResolver hemoAdequacyResolver;

        public CoreReportResolver(
            IConfiguration config,
            HemosheetResolver hemosheetResolver,
            HemoAdequacyResolver hemoRecordResolver)
        {
            this.config = config;
            this.hemosheetResolver = hemosheetResolver;
            this.hemoAdequacyResolver = hemoRecordResolver;
        }

        public (Telerik.Reporting.Report doc, IDocResolver resolver) GetReportAndResolver(string report)
        {
            string reportBasePath = BasePath;
            IDocResolver resolver;
            string reportUri;
            switch (report.ToLower())
            {
                case "hemosheet":
                    reportUri = Path.Combine(reportBasePath, config["Reports:Hemosheet"]);
                    resolver = hemosheetResolver;
                    break;
                case "hemorecords":
                    reportUri = Path.Combine(reportBasePath, config["Reports:HemoRecords"]);
                    resolver = hemoAdequacyResolver;
                    break;
                default: throw new ArgumentException("Unknown doc type.");
            }

            using var sourceStream = File.OpenRead(reportUri);
            var doc = (Telerik.Reporting.Report)new ReportPackager().UnpackageDocument(sourceStream);

            doc.DocumentName = report;

            return (doc, resolver);
        }



        public InstanceReportSource GetReportSource(Telerik.Reporting.Report report, IDictionary<string, object> parameterValues)
        {
            var source = new InstanceReportSource { ReportDocument = report };
            foreach (var item in parameterValues)
            {
                source.Parameters.Add(item.Key, item.Value);
            }

            return source;
        }

        
    }
}
