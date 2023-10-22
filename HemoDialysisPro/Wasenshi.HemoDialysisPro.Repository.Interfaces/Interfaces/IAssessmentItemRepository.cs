using System;
using System.Collections.Generic;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Repositories.Interfaces.Base;

namespace Wasenshi.HemoDialysisPro.Repositories.Interfaces
{
    public interface IAssessmentItemRepository : IRepository<AssessmentItem, Guid>
    {
        void BulkInsertOrUpdate(IEnumerable<AssessmentItem> items);
    }
}