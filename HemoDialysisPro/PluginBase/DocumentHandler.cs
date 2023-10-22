using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wasenshi.HemoDialysisPro.Models;

namespace Wasenshi.HemoDialysisPro.PluginBase
{
    /// <summary>
    /// This plugin module let you process the documents that would be generated from the system at different stages.
    /// Use it with <see cref="IHemoPro"/> to take advantage of this API.
    /// <br/>
    /// <br/>
    /// You can use <see cref="DocumentHandlerBase"/> as a base class for this.
    /// </summary>
    public interface IDocumentHandler
    {
        /// <summary>
        /// This will get called when hemosheet first got created. (when patient dialysis session started).
        /// If the hemosheet should be sent to hospital/center system, you should also chain call to <see cref="SendHemosheet(HemodialysisRecord)"/>.
        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        Task OnHemosheetCreated(HemodialysisRecord record);
        /// <summary>
        /// This will get called when the hemosheet has been completed, either automatically or manually.
        /// </summary>
        /// <param name="hemosheet"></param>
        /// <returns></returns>
        Task OnHemosheetComplete(HemodialysisRecord hemosheet);
        /// <summary>
        /// This is not used right now. But intended to be used when patient needs to be sent/transferred.
        /// </summary>
        /// <param name="patient"></param>
        /// <returns></returns>
        Task<bool> SendPatientDocument(Patient patient);

        /// <summary>
        /// This is called when the hemosheet needs to be sent to hospital/center system.
        /// <br/>
        /// (either by chain-called from <see cref="OnHemosheetComplete(HemodialysisRecord)"/> or manually instructed from somewhere else.
        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        Task<bool> SendHemosheet(HemodialysisRecord record);

        /// <summary>
        /// This will get called whenever a hemosheet is being issued a report.
        /// </summary>
        /// <param name="hemosheet"></param>
        /// <returns></returns>
        Task OnHemosheetMapping(HemodialysisRecord hemosheet, Dictionary<string, object> extra);
    }

    /// <summary>
    /// A base class for processing documentation.
    /// </summary>
    public abstract class DocumentHandlerBase : IDocumentHandler
    {
        protected readonly IHemoPro hemoPro;
        protected readonly ILogger<DocumentHandlerBase> logger;

        protected DocumentHandlerBase(IHemoPro hemoPro, ILogger<DocumentHandlerBase> logger)
        {
            this.hemoPro = hemoPro;
            this.logger = logger;
        }

        public virtual Task OnHemosheetComplete(HemodialysisRecord hemosheet)
        {
            logger.LogInformation("[PLUGIN] no handler override for hemosheet complete.");
            return Task.CompletedTask;
        }

        public virtual Task OnHemosheetCreated(HemodialysisRecord record)
        {
            logger.LogInformation("[PLUGIN] no handler override for hemosheet created.");
            return Task.CompletedTask;
        }

        public virtual async Task<bool> SendHemosheet(HemodialysisRecord record)
        {
            var result = await SendHemosheetPdf(record, await GetHemosheetPdf(record));
            if (result)
            {
                await hemoPro.MarkHemosheetAsSent(record.Id);
            }
            return result;
        }

        protected virtual Task<bool> SendHemosheetPdf(HemodialysisRecord hemosheet, byte[] hemosheetPdf)
        {
            logger.LogInformation("[PLUGIN] no handler override for sending hemosheet.");
            return Task.FromResult(false);
        }

        public virtual Task<bool> SendPatientDocument(Patient patient)
        {
            logger.LogInformation("[PLUGIN] no handler override for sending patient doc.");
            return Task.FromResult(false);
        }

        public virtual Task OnHemosheetMapping(HemodialysisRecord hemosheet, Dictionary<string, object> extra)
        {
            logger.LogInformation("[PLUGIN] no Extra Hemosheet Mapping.");
            return Task.CompletedTask;
        }

        // =========== Utils ==============

        protected async Task<byte[]> GetHemosheetPdf(HemodialysisRecord record)
        {
            var pdf = await hemoPro.GenerateHemosheetPdf(record.Id);
            return pdf;
        }

        protected async Task<byte[]> GetAdequacyPdf(string patientId, DateOnly month)
        {
            var pdf = await hemoPro.GenerateHemoAdequacyPdf(patientId, month);
            return pdf;
        }
        
    }
}
