using AutoMapper;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Models.Stat;
using Wasenshi.HemoDialysisPro.PluginBase.Stats;
using Wasenshi.HemoDialysisPro.Repositories.Interfaces;
using Wasenshi.HemoDialysisPro.Repositories.Interfaces.Base;
using Wasenshi.HemoDialysisPro.Services.Interfaces.Base;

namespace Wasenshi.HemoDialysisPro.Services.BusinessLogic
{
    public interface IDialysisStat : IApplicationService
    {
        TableResult<StatInfo> GetDialysistStat(string duration, string patientId, DateTime? pointOfTime, int? unitId = null);
    }

    public class DialysisStat : StatInfoProcessor<DialysisStatData, List<DialysisStatData>>, IDialysisStat
    {
        private readonly IHemoUnitOfWork hemoUnit;
        private readonly IRepository<DialysisRecord, Guid> dialysisRecordRepo;

        public DialysisStat(
            IHemoUnitOfWork hemoUnit,
            IRepository<DialysisRecord, Guid> dialysisRecordRepo,
            IStatProcessor processor): base(processor)
        {
            this.hemoUnit = hemoUnit;
            this.dialysisRecordRepo = dialysisRecordRepo;
        }

        // =============================== Get Dialysis Stats ==============================================

        public TableResult<StatInfo> GetDialysistStat(string duration, string patientId, DateTime? pointOfTime, int? unitId = null)
        {
            return GetStat(duration, pointOfTime, patientId, unitId);
        }

        protected override List<DialysisStatData> GetData(DateTime filter, TimeSpan? interval, string patientId, int? unitId)
        {
            var hemosheetList = hemoUnit.HemoRecord.GetAllWithPatient();
            if (!string.IsNullOrEmpty(patientId))
            {
                hemosheetList = hemosheetList.Where(h => h.Patient.Id == patientId);
            }
            else if (unitId.HasValue)
            {
                hemosheetList = hemosheetList.Where(h => h.Patient.UnitId == unitId.Value);
            }
            var sql = from hemo in hemosheetList
                          // join records in dialysisRecordRepo.GetAll(false) on hemo.Id equals records.HemodialysisId // need Tests to see which way is more efficient
                      where hemo.Record.CompletedTime != null
                      select new DialysisStatData { Hemosheet = hemo.Record, DialysisRecords = dialysisRecordRepo.GetAll(false).Where(x => x.HemodialysisId == hemo.Record.Id).ToList(), Entry = hemo.Record.CompletedTime };

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

            var data = sql.ToList();
            return data;
        }

        protected override void CalculateStat(List<DialysisStatData> data,
            List<DataRow<StatInfo>> rows, List<StatRowInfo> infoList, DateTime[] columns,
            Func<List<DialysisStatData>, Dictionary<DateTime, List<DialysisStatData>>> groupingFunc)
        {
            var group = groupingFunc(data);

            // Abnormal W trend
            int curRef = AddRowInfo(StatType.Count, infoList);
            var abnormalRow = FillDataRow("Abnormal Weight", columns, group, (columnData) =>
            {
                var cell = new StatInfo
                {
                    TotalCount = columnData.Count,
                    Count = columnData.Count(x => x.Hemosheet.Dehydration.Abnormal)
                };

                return cell;
            }, curRef);
            rows.Add(abnormalRow);
            // Dry Weight
            AddDefaultCalculationRow(rows, "Dry Weight (kg)", columns, group, infoList, x => x.Hemosheet.DialysisPrescription.DryWeight ?? 0);
            // Pre Total Weight
            AddDefaultCalculationRow(rows, "Pre HD Weight (kg)", columns, group, infoList, x => GetPreWeight(x.Hemosheet.Dehydration));
            // Post Total Weight
            AddDefaultCalculationRow(rows, "Post HD Weight (kg)", columns, group, infoList, x => GetPostWeight(x.Hemosheet.Dehydration));
            // Total UF
            AddDefaultCalculationRow(rows, "Total UF (L)", columns, group, infoList, x => x.DialysisRecords.Max(x => x.UFTotal) ?? 0);
            // Pre BPS
            AddDefaultCalculationRow(rows, "Pre HD BPS", columns, group, infoList, x => x.Hemosheet.PreVitalsign.OrderByDescending(v => v.Timestamp).FirstOrDefault()?.BPS ?? 0);
            // Pre BPD
            AddDefaultCalculationRow(rows, "Pre HD BPD", columns, group, infoList, x => x.Hemosheet.PreVitalsign.OrderByDescending(v => v.Timestamp).FirstOrDefault()?.BPD ?? 0);
            // Pre HR
            AddDefaultCalculationRow(rows, "Pre HD HR (Pulse)", columns, group, infoList, x => x.Hemosheet.PreVitalsign.OrderByDescending(v => v.Timestamp).FirstOrDefault()?.HR ?? 0);
            // Post BPS
            AddDefaultCalculationRow(rows, "Post HD BPS", columns, group, infoList, x => x.Hemosheet.PostVitalsign.OrderByDescending(v => v.Timestamp).FirstOrDefault()?.BPS ?? 0);
            // Post BPD
            AddDefaultCalculationRow(rows, "Post HD BPD", columns, group, infoList, x => x.Hemosheet.PostVitalsign.OrderByDescending(v => v.Timestamp).FirstOrDefault()?.BPD ?? 0);
            // Post HR
            AddDefaultCalculationRow(rows, "Post HD HR (Pulse)", columns, group, infoList, x => x.Hemosheet.PostVitalsign.OrderByDescending(v => v.Timestamp).FirstOrDefault()?.HR ?? 0);
            // Dialyzer
            curRef = AddRowInfo(StatType.Max, infoList);
            var dialyzerRow = FillDataRow("Dialyzer", columns, group, (columnData) =>
            {
                return new StatInfo
                {
                    TotalCount = columnData.Count,
                    Text = columnData.GroupBy(x => x.Hemosheet.DialysisPrescription.Dialyzer).OrderByDescending(v => v.Count()).FirstOrDefault()?.Key ?? ""
                };
            }, curRef);
            rows.Add(dialyzerRow);
            // New Dialyzer
            curRef = AddRowInfo(StatType.Count, infoList);
            var newDialyzerRow = FillDataRow("New Dialyzer Use", columns, group, (columnData) =>
            {
                return new()
                {
                    TotalCount = columnData.Count,
                    Count = columnData.Count(x => x.Hemosheet.Dialyzer.UseNo <= 1)
                };
            }, curRef);
            rows.Add(newDialyzerRow);

            // Dialyzer TVC
            AddDefaultCalculationRow(rows, "TVC (%)", columns, group, infoList, x => x.Hemosheet.Dialyzer.TCV);

            // Temporary Dialysis
            curRef = AddRowInfo(StatType.Count, infoList);
            var tempDialysisRow = FillDataRow("Temporary Dialysis", columns, group, (columnData) =>
            {
                return new StatInfo
                {
                    TotalCount = columnData.Count,
                    Count = columnData.Count(x => x.Hemosheet.DialysisPrescription.Temporary)
                };
            }, curRef);
            rows.Add(tempDialysisRow);
        }

        private static void AddDefaultCalculationRow(List<DataRow<StatInfo>> rows, string rowTitleName, DateTime[] columns,
            Dictionary<DateTime, List<DialysisStatData>> group, List<StatRowInfo> info, Func<DialysisStatData, float> targetFieldData)
        {
            int curRef = AddRowInfo(StatType.Avg, info);
            var row = FillDataRow(rowTitleName, columns, group, (columnData) =>
            {
                return new()
                {
                    Avg = columnData.Average(targetFieldData),
                    Max = columnData.Max(targetFieldData),
                    Min = columnData.Min(targetFieldData)
                };
            }, curRef);
            rows.Add(row);
        }

        private static DataRow<StatInfo> FillDataRow(string rowTitleName, DateTime[] columns,
            Dictionary<DateTime, List<DialysisStatData>> group,
            Func<List<DialysisStatData>, StatInfo> stateCalcFunc, int? curRef = null)
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

        private static int AddRowInfo(StatType type, List<StatRowInfo> infoList)
        {
            var rowInfo = new StatRowInfo { Type = type };
            infoList.Add(rowInfo);
            int curRef = infoList.Count - 1;
            return curRef;
        }

        private static float GetPreWeight(DehydrationRecord dehydration)
        {
            return dehydration.PreTotalWeight > 0 ? dehydration.PreTotalWeight - dehydration.WheelchairWeight - dehydration.ClothWeight : 0;
        }

        private static float GetPostWeight(DehydrationRecord dehydration)
        {
            return dehydration.PostTotalWeight > 0 ? dehydration.PostTotalWeight - dehydration.PostWheelchairWeight - dehydration.ClothWeight : 0;
        }

        
    }

    public struct DialysisStatData : IStatData
    {
        public HemodialysisRecord Hemosheet { get; set; }
        public IEnumerable<DialysisRecord> DialysisRecords { get; set; }
        public DateTime? Entry { get; set; }
    }
}
