using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Repositories.Interfaces;
using Wasenshi.HemoDialysisPro.Repository.Core.Interfaces.Base;

namespace Wasenshi.HemoDialysisPro.Repositories.Repository
{
    public class DialysisRecordRepository : Repository<DialysisRecord, Guid>, IDialysisRecordRepository
    {
        public DialysisRecordRepository(IContextAdapter context) : base(context)
        {
        }

        protected override IQueryable<DialysisRecord> GetQueryWithIncludes()
        {
            return base.GetQueryWithIncludes()
                    .Include(x => x.AssessmentItems);
        }
    }
}
