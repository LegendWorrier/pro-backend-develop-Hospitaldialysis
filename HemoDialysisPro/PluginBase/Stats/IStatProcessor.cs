using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Models.Stat;

namespace Wasenshi.HemoDialysisPro.PluginBase.Stats
{
    public interface IStatProcessor
    {
        TimeZoneInfo tz { get; }

        void GetParam(string duration, DateTime? pointOfTime, out DateTime filter, out TimeSpan? interval);

        void SetGrouping<T>(TimeSpan width, DateTime filter,
            out DateTime[] columns,
            out Func<List<T>, Dictionary<DateTime, List<T>>> groupingFunc,
            out Func<DateTime[], IEnumerable<Column>> convertColFunc)
        where T : IStatData;
    }
}
