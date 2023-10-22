using System;
using System.Collections.Generic;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Repositories.Interfaces.Base;

namespace Wasenshi.HemoDialysisPro.Repositories.Interfaces
{
    public interface IDialysisRecordAssessmentItemRepository : IRepository<DialysisRecordAssessmentItem, Guid>
    {
        void BulkInsertOrUpdate(IEnumerable<DialysisRecordAssessmentItem> items);
    }
}