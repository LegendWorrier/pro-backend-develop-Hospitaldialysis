using AutoMapper;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Models.Enums;
using Wasenshi.HemoDialysisPro.Models.Infrastructor;
using Wasenshi.HemoDialysisPro.Repositories.Interfaces;
using Wasenshi.HemoDialysisPro.Services.Interfaces.Base;
using static Wasenshi.HemoDialysisPro.Services.BusinessLogic.StatProcessor;
using Wasenshi.HemoDialysisPro.Utils;
using Wasenshi.HemoDialysisPro.PluginBase.Stats;

namespace Wasenshi.HemoDialysisPro.Services.BusinessLogic
{
    public interface IAssessmentStat : IApplicationService
    {
        TableResult<int> GetAssessmentStat(string duration, string patientId, DateTime? pointOfTime, int? unitId = null);
    }

    public class AssessmentStat : IAssessmentStat
    {
        private readonly IHemoUnitOfWork hemoUnit;
        private readonly IAssessmentRepository assessment;
        private readonly IAssessmentItemRepository assessmentItem;
        private readonly IStatProcessor processor;
        private readonly IMapper mapper;

        public AssessmentStat(
            IHemoUnitOfWork hemoUnit,
            IAssessmentRepository assessment,
            IAssessmentItemRepository assessmentItem,
            IStatProcessor processor,
            IMapper mapper)
        {
            this.hemoUnit = hemoUnit;
            this.assessment = assessment;
            this.assessmentItem = assessmentItem;
            this.processor = processor;
            this.mapper = mapper;
        }

        // =============================== Get Assessment Stats ==============================================

        public TableResult<int> GetAssessmentStat(string duration, string patientId, DateTime? pointOfTime, int? unitId = null)
        {
            processor.GetParam(duration, pointOfTime, out DateTime filter, out TimeSpan? interval);

            var hemosheetsList = hemoUnit.HemoRecord.GetAllWithPatient(false);
            if (!string.IsNullOrEmpty(patientId))
            {
                hemosheetsList = hemosheetsList.Where(x => x.Patient.Id == patientId);
            }
            else if (unitId.HasValue)
            {
                hemosheetsList = hemosheetsList.Where(x => x.Patient.UnitId == unitId.Value);
            }
            var sql = from item in assessmentItem.GetAll()
                      join a in assessment.GetAll() on item.AssessmentId equals a.Id
                      join h in hemosheetsList on item.HemosheetId equals h.Record.Id into hemosheets
                      from hemo in hemosheets.DefaultIfEmpty()
                      where hemo.Record.CompletedTime != null && item.IsActive && a.IsActive && a.Type == AssessmentTypes.Post
                      select new AssessmentStatData { Item = item, Assessment = a, Entry = hemo.Record.CompletedTime };

            var lowerLimit = filter.AsUtcDate();
            if (interval.HasValue)
            {
                var upperLimit = lowerLimit.AddTicks(interval.Value.Ticks);
                sql = sql.Where(x => x.Entry > lowerLimit && x.Entry < upperLimit);
            }
            else
            {
                sql = sql.Where(x => x.Entry > lowerLimit);
            }

            var data = sql.AsEnumerable().AsParallel().GroupBy(x => x.Assessment.Id).ToDictionary(x => x.Key, x => x.ToList());

            var rows = new List<DataRow<int>>();
            var offsetTicks = processor.tz.BaseUtcOffset.Ticks;
            var tzNow = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, processor.tz);
            var width = interval ?? tzNow.AddDays(1).Date - filter.AddTicks(offsetTicks);
            var infoList = new List<AssessmentInfo>();
            var cur = 0;

            processor.SetGrouping(width, filter,
                out DateTime[] columns,
                out Func<List<AssessmentStatData>, Dictionary<DateTime, List<AssessmentStatData>>> groupingFunc,
                out Func<DateTime[], IEnumerable<Column>> convertColFunc);
            CalculateStat(data, rows, columns, groupingFunc);

            return new()
            {
                Rows = rows,
                Columns = convertColFunc(columns),
                Info = infoList
            };

            // ---------- Local Function --------------------
            void CalculateStat(Dictionary<long, List<AssessmentStatData>> data,
                List<DataRow<int>> rows,
                DateTime[] columns,
                Func<List<AssessmentStatData>,
                Dictionary<DateTime, List<AssessmentStatData>>> groupingFunc)
            {
                foreach (var item in data.Values)
                {
                    var assess = item[0].Assessment;
                    var group = groupingFunc(item);
                    infoList.Add(mapper.Map<AssessmentInfo>(assess));

                    if (assess.Multi || assess.OptionsList?.Count > 0)
                    {
                        rows.AddRange(MultiOption(assess, columns, group, cur));
                    }
                    else
                    {
                        rows.Add(SingleOption(assess, columns, group, cur));
                    }

                    cur++;
                }
            }
        }

        private static IEnumerable<DataRow<int>> MultiOption(Assessment assessment,
            DateTime[] columns,
            Dictionary<DateTime, List<AssessmentStatData>> group,
            int? curRef)
        {
            var optionList = assessment.OptionsList;
            if (optionList == null) return Enumerable.Empty<DataRow<int>>();
            var optionRows = new Dictionary<long, DataRow<int>>(optionList.Count);
            // filter out no keyword
            foreach (var option in optionList.Where(x => !x.Name.Equals("no", StringComparison.OrdinalIgnoreCase)))
            {
                optionRows.Add(option.Id, new DataRow<int>
                {
                    Title = option.DisplayName,
                    Data = new int[columns.Length],
                    InfoRef = curRef
                });
            }
            // Count each assessments tick
            int col = 0;
            foreach (var id in columns
                .Where((x, i) => { col = i; return group.ContainsKey(x); })
                .SelectMany(x => group[x]) // AssessmentStatData
                .SelectMany(x => x.Item.Selected.Where(x => optionRows.ContainsKey(x))))
            {
                optionRows[id].Data[col]++;
            }

            return optionRows.Select(x => x.Value);
        }

        private static DataRow<int> SingleOption(Assessment assessment,
            DateTime[] columns,
            Dictionary<DateTime, List<AssessmentStatData>> group,
            int? curRef)
        {
            var name = assessment.DisplayName;
            var data = new int[columns.Length];
            // Count assessment tick
            int col = 0;
            foreach (List<AssessmentStatData> items in columns
                .Where((x, i) => { col = i; return group.ContainsKey(x); })
                .Select(x => group[x])) // AssessmentStatData list
            {
                data[col] = items.Count(x => x.Item.Checked);
            }

            return new DataRow<int>
            {
                Title = name,
                Data = data,
                InfoRef = curRef
            };
        }
    }

    public struct AssessmentStatData : IStatData
    {
        public AssessmentItem Item { get; set; }
        public Assessment Assessment { get; set; }
        public DateTime? Entry { get; set; }
    }
}
