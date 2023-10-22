using System;
using System.Collections.Generic;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Repositories.Interfaces;
using Wasenshi.HemoDialysisPro.Repository.Core.Interfaces.Base;

namespace Wasenshi.HemoDialysisPro.Repositories.Repository
{
    public class DialysisRecordAssessmentItemRepository : Repository<DialysisRecordAssessmentItem, Guid>, IDialysisRecordAssessmentItemRepository
    {
        public DialysisRecordAssessmentItemRepository(IContextAdapter context) : base(context)
        {
        }

        public void BulkInsertOrUpdate(IEnumerable<DialysisRecordAssessmentItem> items)
        {
            context.DialysisRecordAssessmentItems.UpdateRange(items);
        }
    }
}
