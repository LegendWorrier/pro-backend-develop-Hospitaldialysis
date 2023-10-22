using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Services.Interfaces.Base;
using Wasenshi.HemoDialysisPro.Models;

namespace Wasenshi.HemoDialysisPro.Services
{
    public interface IMedHistoryService : IApplicationService
    {
        Page<MedHistoryItem> GetAllMedHistory(int page = 1, int limit = 25,
            Action<IOrderer<MedHistoryItem>> orderBy = null,
            Expression<Func<MedHistoryItem, bool>> condition = null);
        MedHistoryItem GetMedHistory(Guid id);
        IEnumerable<MedHistoryItem> CreateMedHistoryBatch(string patientId, DateTime entryTime, List<MedHistoryItem> medItems);
        MedHistoryItem UpdateMedHistory(MedHistoryItem medItem);
        bool DeleteMedHistory(Guid id);

        MedHistoryResult GetMedHistoryByPatientId(string patientId, Expression<Func<MedHistoryItem, bool>> prerequisite = null, DateTime? filter = null, DateTime? upperLimit = null);
        MedOverview GetMedOverviewByPatientId(string patientId);
    }
}