using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Models.TimezoneUtil;
using Wasenshi.HemoDialysisPro.Services.Interfaces;

namespace Wasenshi.HemoDialysisPro.Services.BusinessLogic
{
    public class LabExamProcessor : ILabExamProcessor
    {
        private readonly IConfiguration config;

        public LabExamProcessor(IConfiguration config)
        {
            this.config = config;
        }

        private int CheckLimit(LabExamItem labItem, float? value, Patient patient)
        {
            if (value == null)
            {
                return 0;
            }

            var limit = GetLimit(labItem, patient);
            if (limit.upper != null && value > limit.upper)
            {
                return 1;
            }

            if (limit.lower != null && value < limit.lower)
            {
                return -1;
            }

            return 0;
        }

        private (float? upper, float? lower) GetLimit(LabExamItem labItem, Patient patient)
        {
            var upperLimit = labItem.UpperLimit;
            var lowerLimit = labItem.LowerLimit;
            if (patient.Gender == "M")
            {
                upperLimit = labItem.UpperLimitM ?? upperLimit;
                lowerLimit = labItem.LowerLimitM ?? lowerLimit;
            }
            else if (patient.Gender == "F")
            {
                upperLimit = labItem.UpperLimitF ?? upperLimit;
                lowerLimit = labItem.LowerLimitF ?? lowerLimit;
            }

            return (upperLimit, lowerLimit);
        }

        public LabExamResult ProcessData(IEnumerable<LabExam> labExams)
        {
            TimeZoneInfo tz = TimezoneUtils.GetTimeZone(config.GetValue<string>("TIMEZONE"));
            var offsetTicks = tz.BaseUtcOffset.Ticks;

            var group = labExams.GroupBy(x => x.LabItem.Name)
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
                return new LabExamResult
                {
                    Data = new KeyValuePair<LabExamItem, List<LabExam>[]>[0],
                    Columns = new DateTime[0]
                };
            }

            var column = group.Select(x => x.Data.Select(y => TimeZoneInfo.ConvertTime(new DateTimeOffset(y.EntryTime, TimeSpan.Zero), tz).Date.AddTicks(-offsetTicks)))
                .Aggregate((a, b) => a.Concat(b))
                .Distinct()
                .OrderByDescending(x => x)
                .ToList();
            var records = new List<KeyValuePair<LabExamItem, List<LabExam>[]>>();

            foreach (var record in group)
            {
                var labList = new Queue<LabExam>(record.Data);
                var labItem = labList.Peek().LabItem;
                var data = new List<LabExam>[column.Count];
                for (int i = 0; i < column.Count; i++)
                {
                    var entry = column[i];
                    data[i] = new List<LabExam>();
                    while (labList.Count > 0 && SameDay(offsetTicks, labList.Peek().EntryTime, entry))
                    {
                        data[i].Add(labList.Dequeue());
                    }
                    data[i].Reverse();
                }

                records.Add(new KeyValuePair<LabExamItem, List<LabExam>[]>(labItem, data));
            }

            LabExamResult result = new LabExamResult
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
