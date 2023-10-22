using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Models.Stat;
using Wasenshi.HemoDialysisPro.PluginBase.Stats;

namespace Wasenshi.HemoDialysisPro.PluginBase
{
    /// <summary>
    /// This plugin module allow you to extends the stat result.
    /// </summary>
    public interface IStatHandler
    {
        TableResult<object> GetStat(string name, string duration, DateTimeOffset? pointOfTime = null, int? unitId = null, string patientId = null);

        /// <summary>
        /// This need to be implemented and return all the stat available in this plugin, So that FE can list it for you.
        /// </summary>
        /// <returns></returns>
        StatItem[] GetCustomStatList();
    }

    /// <summary>
    /// Base class for processing universal stat in hemopro system.
    /// </summary>
    public abstract class StatHandlerBase : IStatHandler
    {
        protected readonly ILogger<StatHandlerBase> logger;
        protected readonly IStatProcessor processor;

        protected StatHandlerBase(ILogger<StatHandlerBase> logger, IStatProcessor processor)
        {
            this.logger = logger;
            this.processor = processor;
        }

        public virtual StatItem[] GetCustomStatList()
        {
            return Array.Empty<StatItem>();
        }

        public abstract TableResult<object> GetStat(string name, string duration, DateTimeOffset? pointOfTime = null, int? unitId = null, string patientId = null);
    }

    /// <summary>
    /// Base class for processing any stat in hemopro system and utilize <see cref="StatInfo"/>.
    /// </summary>
    public abstract class StatInfoHandlerBase : IStatHandler
    {
        protected readonly ILogger<StatInfoHandlerBase> logger;
        protected readonly IStatInfoProcessor processor;

        protected StatInfoHandlerBase(ILogger<StatInfoHandlerBase> logger, IStatInfoProcessor processor)
        {
            this.logger = logger;
            this.processor = processor;
        }

        public virtual StatItem[] GetCustomStatList()
        {
            return Array.Empty<StatItem>();
        }

        public abstract TableResult<object> GetStat(string name, string duration, DateTimeOffset? pointOfTime = null, int? unitId = null, string patientId = null);
    }
}
