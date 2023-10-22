using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wasenshi.HemoDialysisPro.Models.Stat;
using Wasenshi.HemoDialysisPro.Models;

namespace Wasenshi.HemoDialysisPro.PluginBase.Stats
{
    public interface IStatInfoProcessor
    {
        delegate void CalculateStatInfo<T, TInput>(TInput data,
                List<DataRow<StatInfo>> rows,
                List<StatRowInfo> infoList,
                DateTime[] columns,
                Func<List<T>,
                Dictionary<DateTime, List<T>>> groupingFunc)
            where T : IStatData;

        TableResult<StatInfo> ProcessData<T, TInput>(DateTime filter, TimeSpan? interval, TInput data, CalculateStatInfo<T, TInput> calculateStat)
            where T : IStatData;
    }
}
