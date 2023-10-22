using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using Telerik.Reporting;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Repositories.Interfaces;
using Wasenshi.HemoDialysisPro.Models.TimezoneUtil;
using System.Threading.Tasks;

namespace Wasenshi.HemoDialysisPro.Report.DocumentLogics
{
    public abstract class DocResolverBase : IDocResolver
    {
        protected readonly IFileRepository fileRepo;
        protected readonly IConfiguration config;
        protected readonly ILogger<IDocResolver> logger;
        protected readonly IUserResolver userResolver;

        protected TimeZoneInfo tz;

        public abstract object GetData(object data);

        public abstract Task<object> PrepareData(IDictionary<string, object> parameters);

        public abstract Task<object> UpdateData(object prevData);

        public virtual void ExtraSetup(InstanceReportSource report)
        {
            // default: do nothing
        }

        protected DocResolverBase(
            ILogger<IDocResolver> logger,
            IUserResolver userResolver,
            IFileRepository fileRepo,
            IConfiguration config)
        {
            this.logger = logger;
            this.userResolver = userResolver;
            this.fileRepo = fileRepo;
            this.config = config;
            tz = TimezoneUtils.GetTimeZone(config.GetValue<string>("TIMEZONE"));
        }

        protected string GetLogo()
        {
            FileEntry file = fileRepo.Get("logo");
            if (file == null)
            {
                return null;
            }
            var uri = Path.Combine(CoreReportResolver.uploadsFolder, file.Uri);

            return uri;
        }

        protected string GetSignature(Guid userId)
        {
            var signatureId = userResolver.Repo.Get(userId).Signature;
            FileEntry file = fileRepo.Get(signatureId);
            if (file == null)
            {
                return null;
            }
            var uri = Path.Combine(CoreReportResolver.uploadsFolder, file.Uri);

            return uri;
        }
    }
}
