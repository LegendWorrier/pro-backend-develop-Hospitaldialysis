using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Models.Stat;
using Wasenshi.HemoDialysisPro.Models.TimezoneUtil;
using Wasenshi.HemoDialysisPro.PluginBase.Stats;
using Wasenshi.HemoDialysisPro.Utils;

namespace Wasenshi.HemoDialysisPro.Services.BusinessLogic
{
    public class StatProcessor : IStatProcessor
    {
        protected readonly IConfiguration config;

        public TimeZoneInfo tz { get; }

        public StatProcessor(
            IConfiguration config)
        {
            this.config = config;

            tz = TimezoneUtils.GetTimeZone(this.config.GetValue<string>("TIMEZONE"));
        }

        protected const int monthsThreshold = 31;
        protected const int yearsThreshold = 366 * 2;

        public void SetGrouping<T>(TimeSpan width, DateTime filter,
            out DateTime[] columns,
            out Func<List<T>, Dictionary<DateTime, List<T>>> groupingFunc,
            out Func<DateTime[], IEnumerable<Column>> convertColFunc)
        where T : IStatData
        {
            var offsetTicks = tz.BaseUtcOffset.Ticks;

            // day by day (a month)
            if ((int)width.TotalDays <= monthsThreshold)
            {
                var init = TimeZoneInfo.ConvertTime(new DateTimeOffset(filter, TimeSpan.Zero), tz);
                columns = Enumerable.Range(0, (int)width.TotalDays).Select(i => init.AddDays(i).ToUniversalTime().DateTime).ToArray();
                convertColFunc = (columns) => columns.Select(x => new Column { Title = x.AddTicks(offsetTicks).ToString("M"), Data = new DateTimeOffset(x, TimeSpan.Zero) });
                groupingFunc = (List<T> item) =>
                    item.AsParallel()
                        .GroupBy(x => x.Entry.Value.AddTicks(offsetTicks).Date)
                        .OrderBy(x => x.Key)
                        .ToDictionary(x => x.Key.AddTicks(-offsetTicks), x => x.ToList());
            }
            // month by month (a year)
            else if ((int)width.TotalDays <= yearsThreshold)
            {
                var dTz = TimeZoneInfo.ConvertTime(new DateTimeOffset(filter, TimeSpan.Zero), tz);
                var init = dTz.AddDays(1 - dTz.Day).AddTicks(-dTz.TimeOfDay.Ticks); // ensure point to 1st day of the month
                columns = Enumerable.Range(0, (int)width.TotalDays / 30).Select(i => init.AddMonths(i).ToUniversalTime().DateTime).ToArray();
                convertColFunc = (columns) => columns.Select(x => new Column { Title = x.AddTicks(offsetTicks).ToString("Y"), Data = new DateTimeOffset(x, TimeSpan.Zero) });
                groupingFunc = (List<T> item) =>
                    item.AsParallel()
                        .GroupBy(x => new DateTime(x.Entry.Value.AddTicks(offsetTicks).Year, x.Entry.Value.AddTicks(offsetTicks).Month, 1))
                        .OrderBy(x => x.Key)
                        .ToDictionary(x => x.Key.AddTicks(-offsetTicks), x => x.ToList());
            }
            // year by year (multi-year)
            else
            {
                var dTz = TimeZoneInfo.ConvertTime(new DateTimeOffset(filter, TimeSpan.Zero), tz);
                var init = dTz.AddDays(1 - dTz.Day).AddTicks(-dTz.TimeOfDay.Ticks).AddMonths(1 - dTz.Month); // ensure point to 1st day of the year
                columns = Enumerable.Range(0, (int)width.TotalDays / 365).Select(i => init.AddYears(i).ToUniversalTime().DateTime).ToArray();
                convertColFunc = (columns) => columns.Select(x => new Column { Title = x.AddTicks(offsetTicks).ToString("yyyy"), Data = new DateTimeOffset(x, TimeSpan.Zero) });
                groupingFunc = (List<T> item) =>
                    item.AsParallel()
                        .GroupBy(x => new DateTime(x.Entry.Value.AddTicks(offsetTicks).Year, 1, 1))
                        .OrderBy(x => x.Key)
                        .ToDictionary(x => x.Key.AddTicks(-offsetTicks), x => x.ToList());
            }
        }

        public void GetParam(string duration, DateTime? pointOfTime, out DateTime filter, out TimeSpan? interval)
        {
            ParseDuration(duration, out int i, out Duration type);

            if (pointOfTime.HasValue)
            {
                filter = pointOfTime.Value;
                interval = GetIntervalFromFilter(type, filter, i);
            }
            else
            {
                CalculateRange(type, i, out interval, out filter);
            }
        }

        private TimeSpan GetIntervalFromFilter(Duration type, DateTime filter, int duration)
        {
            var dTz = TimeZoneInfo.ConvertTime(new DateTimeOffset(filter, TimeSpan.Zero), tz);
            return type switch
            {
                Duration.Day => dTz.AddDays(duration) - filter,
                Duration.Month => dTz.AddMonths(duration) - filter,
                Duration.Year => dTz.AddYears(duration > 1 ? duration : 1) - filter,
                _ => throw new InvalidProgramException(),
            };
        }

        private void CalculateRange(Duration type, int duration, out TimeSpan? interval, out DateTime filter)
        {
            var tzNow = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, tz);
            switch (type)
            {
                case Duration.Day:
                    interval = null;
                    filter = new DateTimeOffset(tzNow.Year, tzNow.Month, tzNow.Day, 0, 0, 0, tz.BaseUtcOffset).AddDays(-(duration - 1)).UtcDateTime;
                    break;
                case Duration.Month:
                    {
                        var upper = (duration > 1) ?
                        tzNow.AddTicks(-tzNow.TimeOfDay.Ticks).AddDays(1 - tzNow.Day).AddMonths(1) :
                        tzNow.AddTicks(-tzNow.TimeOfDay.Ticks).AddDays(1);
                        filter = (duration > 1) ?
                            new DateTimeOffset(tzNow.Year, tzNow.Month, 1, 0, 0, 0, tz.BaseUtcOffset).AddMonths(-(duration - 1)).UtcDateTime :
                            upper.AddDays(-DateTime.DaysInMonth(tzNow.Year, tzNow.Month)).UtcDateTime;

                        interval = upper - filter;
                    }
                    break;
                case Duration.Year:
                    {
                        const int threshold = 2; // this threshold must conform with the column grouping logic in the stat itself

                        var upper = (duration > threshold) ?
                            tzNow.AddDays(1 - tzNow.Day).AddMonths(1 - tzNow.Month).AddTicks(-tzNow.TimeOfDay.Ticks).AddYears(1) :
                            tzNow.AddDays(1 - tzNow.Day).AddMonths(1).AddTicks(-tzNow.TimeOfDay.Ticks);
                        filter = (duration > threshold) ?
                            new DateTimeOffset(tzNow.Year, 1, 1, 0, 0, 0, tz.BaseUtcOffset).AddYears(-(duration - 1)).UtcDateTime :
                            upper.AddYears(-duration).UtcDateTime;

                        interval = upper - filter;
                    }
                    break;
                default:
                    throw new InvalidProgramException();
            }
        }

        private static void ParseDuration(string durationStr, out int duration, out Duration type)
        {
            if (!int.TryParse(durationStr.Remove(durationStr.Length - 1), out duration) && durationStr.Length > 1)
            {
                throw new AppException("BAD_REQUEST", $"Cannot parse the duration. [invalid duration: {durationStr}]");
            }
            duration = Math.Max(1, duration); // duration cannot be lower than 1
            // duration type : D, M, Y
            type = durationStr[^1..] switch
            {
                "D" => Duration.Day,
                "M" => Duration.Month,
                "Y" => Duration.Year,
                _ => throw new AppException("BAD_REQUEST", $"Cannot parse the duration. [Unknown duration code: {durationStr}]"),
            };
        }

        private enum Duration
        {
            Day,
            Month,
            Year
        }
    }

    public class StatInfoProcessor : IStatInfoProcessor
    {
        protected readonly IStatProcessor processor;

        public StatInfoProcessor(IStatProcessor processor)
        {
            this.processor = processor;
        }

        public TableResult<StatInfo> ProcessData<T, TInput>(DateTime filter, TimeSpan? interval, TInput data, IStatInfoProcessor.CalculateStatInfo<T, TInput> calculateStat) where T : IStatData
        {
            var offsetTicks = processor.tz.BaseUtcOffset.Ticks;
            var tzNow = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, processor.tz);

            var rows = new List<DataRow<StatInfo>>();
            var width = interval ?? tzNow.AddDays(1).Date - filter.AddTicks(offsetTicks);
            List<StatRowInfo> infoList = new();
            processor.SetGrouping(width, filter,
                out DateTime[] columns,
                out Func<List<T>, Dictionary<DateTime, List<T>>> groupingFunc,
                out Func<DateTime[], IEnumerable<Column>> convertColFunc);
            calculateStat(data, rows, infoList, columns, groupingFunc);

            return new()
            {
                Rows = rows,
                Columns = convertColFunc(columns),
                Info = infoList
            };
        }
    }

    public abstract class StatInfoProcessor<T, TInput> : StatInfoProcessor
        where T : IStatData
    {
        protected StatInfoProcessor(IStatProcessor processor) : base(processor)
        {
        }

        public virtual TableResult<StatInfo> GetStat(string duration, DateTime? pointOfTime, string patientId, int? unitId)
        {
            processor.GetParam(duration, pointOfTime, out DateTime filter, out TimeSpan? interval);
            var data = GetData(filter, interval, patientId, unitId);

            return ProcessData(filter, interval, data);
        }

        protected abstract TInput GetData(DateTime filter, TimeSpan? interval, string patientId, int? unitId);

        protected abstract void CalculateStat(TInput data,
                List<DataRow<StatInfo>> rows,
                List<StatRowInfo> infoList,
                DateTime[] columns,
                Func<List<T>,
                Dictionary<DateTime, List<T>>> groupingFunc);

        protected TableResult<StatInfo> ProcessData(DateTime filter, TimeSpan? interval, TInput data)
        {
            return ProcessData<T, TInput>(filter, interval, data, CalculateStat);
        }
    }
}
