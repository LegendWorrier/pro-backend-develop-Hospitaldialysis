using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Repositories.Interfaces;
using Wasenshi.HemoDialysisPro.Repository.Core.Interfaces.Base;

namespace Wasenshi.HemoDialysisPro.Repositories.Repository
{
    public class MedHistoryRepository : Repository<MedHistoryItem, Guid>, IMedHistoryRepository
    {
        public MedHistoryRepository(IContextAdapter context) : base(context)
        {
        }

        protected override IQueryable<MedHistoryItem> GetQueryWithIncludes()
        {
            return base.GetQueryWithIncludes()
                .Include(x => x.Medicine);
        }

        public void CreateBatch(IEnumerable<MedHistoryItem> medItems)
        {
            context.MedicineHistories.AddRange(medItems);
        }
    }
}
