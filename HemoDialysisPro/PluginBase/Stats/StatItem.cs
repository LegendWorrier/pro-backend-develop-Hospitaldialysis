using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wasenshi.HemoDialysisPro.PluginBase.Stats
{
    public class StatItem
    {
        public string DisplayName { get; set; }
        public string PageName { get; set; }
        /// <summary>
        /// This is the main name of this stat, it will be bound to route and stat selection
        /// </summary>
        public string Name { get; set; }
        public string ExcelFileName { get; set; }

        public bool HasPatient { get; set; }

        public Dictionary<string, object> ExtraParams { get; set; } = new Dictionary<string, object>();

    }
}
