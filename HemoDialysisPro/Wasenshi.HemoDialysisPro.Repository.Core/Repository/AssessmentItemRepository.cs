using System;
using System.Collections.Generic;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Repositories.Interfaces;
using Wasenshi.HemoDialysisPro.Repository.Core.Interfaces.Base;

namespace Wasenshi.HemoDialysisPro.Repositories.Repository
{
    public class AssessmentItemRepository : Repository<AssessmentItem, Guid>, IAssessmentItemRepository
    {
        public AssessmentItemRepository(IContextAdapter context) : base(context)
        {
        }

        public void BulkInsertOrUpdate(IEnumerable<AssessmentItem> items)
        {
            context.AssessmentItems.UpdateRange(items);
        }
    }
}
