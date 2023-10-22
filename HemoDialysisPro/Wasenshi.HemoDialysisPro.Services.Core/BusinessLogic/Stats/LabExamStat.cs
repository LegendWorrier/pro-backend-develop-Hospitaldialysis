using AutoMapper;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Models.Stat;
using Wasenshi.HemoDialysisPro.PluginBase.Stats;
using Wasenshi.HemoDialysisPro.Repositories.Interfaces;
using Wasenshi.HemoDialysisPro.Services.Interfaces.Base;

namespace Wasenshi.HemoDialysisPro.Services.BusinessLogic
{
    public interface ILabExamStat : IApplicationService
    {
        TableResult<StatInfo> GetLabExamStat(string duration, DateTime? pointOfTime, int? unitId = null);
    }

    public class LabExamStat : StatInfoProcessor<LabStat, Dictionary<int, List<LabStat>>>, ILabExamStat
    {
        private readonly ILabUnitOfWork labUow;
        private readonly IPatientRepository patientRepository;

        public LabExamStat(
            ILabUnitOfWork labUow,
            IPatientRepository patientRepository,
            IStatProcessor processor) : base(processor)
        {
            this.labUow = labUow;
            this.patientRepository = patientRepository;
        }

        // =============================== Get Lab Exam Stats ==============================================

        public TableResult<StatInfo> GetLabExamStat(string duration, DateTime? pointOfTime, int? unitId = null)
        {
            return GetStat(duration, pointOfTime, null, unitId);
        }

        protected override Dictionary<int, List<LabStat>> GetData(DateTime filter, TimeSpan? interval, string patientId, int? unitId)
        {
            var labList = labUow.LabExam.GetAll();
            if (unitId.HasValue)
            {
                labList = labList.Join(patientRepository.GetAll(false), l => l.PatientId, p => p.Id, (l, p) => new { Lab = l, Patient = p })
                    .Where(x => x.Patient.UnitId == unitId)
                    .Select(x => x.Lab);
            }
            var sql = from l in labList select new LabStat { Lab = l, Entry = l.EntryTime };

            var lowerLimit = filter;
            if (interval.HasValue)
            {
                var upperLimit = lowerLimit.AddTicks(interval.Value.Ticks);
                sql = sql.Where(x => x.Entry > lowerLimit && x.Entry < upperLimit);
            }
            else
            {
                sql = sql.Where(x => x.Entry > lowerLimit);
            }

            var data = sql.AsEnumerable().AsParallel().GroupBy(x => x.Lab.LabItemId).ToDictionary(x => x.Key, x => x.ToList());
            return data;
        }

        protected override void CalculateStat(Dictionary<int, List<LabStat>> data,
            List<DataRow<StatInfo>> rows, List<StatRowInfo> infoList, DateTime[] columns,
            Func<List<LabStat>, Dictionary<DateTime, List<LabStat>>> groupingFunc)
        {
            foreach (var item in data.Values)
            {
                var labItem = item[0].Lab.LabItem;

                if (labItem.IsSystemBound && labItem.Bound == Models.Enums.SpecialLabItem.BUN)
                {
                    var group = item.GroupBy(x => x.Lab.PatientId);
                    var preBUN = group.Select(x => x.OrderBy(l => l.Entry.Value).Take(1).FirstOrDefault()).Where(x => x.Entry != null && x.Lab != null).ToList();
                    var postBUN = group.Select(x => x.OrderBy(l => l.Entry.Value).Skip(1).LastOrDefault()).Where(x => x.Entry != null && x.Lab != null).ToList();
                    AddDefaultCalculationRow(rows, labItem, columns, groupingFunc(preBUN), infoList, x => x.Lab.LabValue, "Pre BUN");
                    AddDefaultCalculationRow(rows, labItem, columns, groupingFunc(postBUN), infoList, x => x.Lab.LabValue, "Post BUN");
                }
                else
                {
                    var group = groupingFunc(item);
                    AddDefaultCalculationRow(rows, labItem, columns, group, infoList, x => x.Lab.LabValue);
                }
            }
        }

        private static void AddDefaultCalculationRow(List<DataRow<StatInfo>> rows, LabExamItem labItem, DateTime[] columns,
            Dictionary<DateTime, List<LabStat>> group, List<StatRowInfo> info, Func<LabStat, float> targetFieldData, string titleName = null)
        {
            int curRef = AddRowInfo(labItem, info);
            var row = FillDataRow(titleName ?? labItem.Name, columns, group, (columnData) =>
            {
                var cell = new StatInfo
                {
                    Avg = columnData.Average(targetFieldData),
                    Max = columnData.Max(targetFieldData),
                    Min = columnData.Min(targetFieldData)
                };
                return cell;
            }, curRef);
            rows.Add(row);
        }

        private static DataRow<StatInfo> FillDataRow(string rowTitleName, DateTime[] columns,
            Dictionary<DateTime, List<LabStat>> group,
            Func<List<LabStat>, StatInfo> stateCalcFunc, int? curRef = null)
        {
            var row = new DataRow<StatInfo>
            {
                Title = rowTitleName,
                Data = new StatInfo[columns.Length],
                InfoRef = curRef
            };
            for (int i = 0; i < columns.Length; i++)
            {
                if (!group.ContainsKey(columns[i]))
                {
                    continue;
                }

                var columnData = group[columns[i]];

                // custom calc func
                var cell = stateCalcFunc(columnData);
                row.Data[i] = cell;
            }

            return row;
        }

        private static int AddRowInfo(LabExamItem labExamItem, List<StatRowInfo> infoList)
        {
            var rowInfo = new StatRowInfo { Type = StatType.Avg, Info = labExamItem };
            infoList.Add(rowInfo);
            int curRef = infoList.Count - 1;
            return curRef;
        }
    }

    public struct LabStat : IStatData
    {
        public LabExam Lab { get; set; }
        public DateTime? Entry { get; set; }
    }
}
