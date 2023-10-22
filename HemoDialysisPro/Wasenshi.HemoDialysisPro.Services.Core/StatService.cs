using Microsoft.Extensions.Configuration;
using System;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Models.Stat;
using Wasenshi.HemoDialysisPro.Services.BusinessLogic;
using Wasenshi.HemoDialysisPro.Services.Interfaces;
using Wasenshi.HemoDialysisPro.Models.TimezoneUtil;
using Wasenshi.HemoDialysisPro.Utils;

namespace Wasenshi.HemoDialysisPro.Services
{
    public class StatService : IStatService
    {
        private readonly IConfiguration config;
        private readonly IAssessmentStat assessmentStat;
        private readonly IDialysisStat dialysisStat;
        private readonly ILabExamStat labExamStat;

        private readonly TimeZoneInfo tz;

        public StatService(
            IConfiguration config,
            IAssessmentStat assessmentStat,
            IDialysisStat dialysisStat,
            ILabExamStat labExamStat)
        {
            this.config = config;
            this.assessmentStat = assessmentStat;
            this.dialysisStat = dialysisStat;
            this.labExamStat = labExamStat;
            tz = TimezoneUtils.GetTimeZone(config.GetValue<string>("TIMEZONE"));
        }

        public TableResult<int> GetAssessmentStat(string duration, DateTime? pointOfTime = null, string patientId = null, int? unitId = null)
        {
            var result = assessmentStat.GetAssessmentStat(duration, patientId, pointOfTime, unitId);

            return result;
        }

        public TableResult<StatInfo> GetDialysisStat(string duration, DateTime? pointOfTime = null, string patientId = null, int? unitId = null)
        {
            var result = dialysisStat.GetDialysistStat(duration, patientId, pointOfTime, unitId);

            return result;
        }

        public TableResult<StatInfo> GetLabExamGlobalStat(string duration, DateTime? pointOfTime = null, int? unitId = null)
        {
            var result = labExamStat.GetLabExamStat(duration, pointOfTime, unitId);

            return result;
        }
        
    }
}
