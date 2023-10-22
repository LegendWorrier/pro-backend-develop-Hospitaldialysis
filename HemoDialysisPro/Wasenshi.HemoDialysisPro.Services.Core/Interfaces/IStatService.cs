using System;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Models.Stat;
using Wasenshi.HemoDialysisPro.Services.Interfaces.Base;

namespace Wasenshi.HemoDialysisPro.Services.Interfaces
{
    public interface IStatService : IApplicationService
    {
        TableResult<int> GetAssessmentStat(string duration, DateTime? pointOfTime = null, string patientId = null, int? unitId = null);
        TableResult<StatInfo> GetDialysisStat(string duration, DateTime? pointOfTime = null, string patientId = null, int? unitId = null);
        TableResult<StatInfo> GetLabExamGlobalStat(string duration, DateTime? pointOfTime = null, int? unitId = null);
    }
}