using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Models.Entity.Stockable;
using Wasenshi.HemoDialysisPro.Services.Interfaces;
using Wasenshi.HemoDialysisPro.Models.TimezoneUtil;

namespace Wasenshi.HemoDialysisPro.Services.BusinessLogic
{
    public class MedHistoryProcessor : IMedHistoryProcessor
    {
        private readonly IConfiguration config;

        public MedHistoryProcessor(IConfiguration config)
        {
            this.config = config;
        }

        public MedHistoryResult ProcessData(IEnumerable<MedHistoryItem> medItems)
        {
            TimeZoneInfo tz = TimezoneUtils.GetTimeZone(config.GetValue<string>("TIMEZONE"));
            var offsetTicks = tz.BaseUtcOffset.Ticks;

            var group = medItems.GroupBy(x => x.Medicine.Name)
                .Select(group =>
                        new
                        {
                            Name = group.Key,
                            Data = group.OrderByDescending(x => x.EntryTime)
                        })
                  .OrderBy(group => group.Name)
                  .ToList();
            if (group.Count == 0)
            {
                return new MedHistoryResult
                {
                    Data = new KeyValuePair<Medicine, List<MedHistoryItem>[]>[0],
                    Columns = new DateTime[0]
                };
            }

            var column = group.Select(x => x.Data.Select(y => TimeZoneInfo.ConvertTime(new DateTimeOffset(y.EntryTime, TimeSpan.Zero), tz).Date.AddTicks(-offsetTicks)))
                .Aggregate((a, b) => a.Concat(b))
                .Distinct()
                .OrderByDescending(x => x)
                .ToList();
            var records = new List<KeyValuePair<Medicine, List<MedHistoryItem>[]>>();

            foreach (var record in group)
            {
                var medList = new Queue<MedHistoryItem>(record.Data);
                var medicine = medList.Peek().Medicine;
                var data = new List<MedHistoryItem>[column.Count];
                for (int i = 0; i < column.Count; i++)
                {
                    var entry = column[i];
                    data[i] = new List<MedHistoryItem>();
                    while (medList.Count > 0 && SameDay(offsetTicks, medList.Peek().EntryTime, entry))
                    {
                        data[i].Add(medList.Dequeue());
                    }
                    data[i].Reverse();
                }

                records.Add(new KeyValuePair<Medicine, List<MedHistoryItem>[]>(medicine, data));
            }

            MedHistoryResult result = new MedHistoryResult
            {
                Columns = column,
                Data = records
            };

            return result;

            static bool SameDay(long offsetTicks, DateTime first, DateTime second)
            {
                var firstTz = first.AddTicks(offsetTicks);
                var secondTz = second.AddTicks(offsetTicks);
                return firstTz.Date == secondTz.Date; // note: this logic-code can only be executed in-memory, not in database
            }
        }
    }
}
