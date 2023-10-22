using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wasenshi.HemoDialysisPro.PluginBase.Stats
{
    /// <summary>
    /// Use this interface for stat data object that needs to be passed to SetGrouping function.
    /// </summary>
    public interface IStatData
    {
        public DateTime? Entry { get; set; }
    }
}
