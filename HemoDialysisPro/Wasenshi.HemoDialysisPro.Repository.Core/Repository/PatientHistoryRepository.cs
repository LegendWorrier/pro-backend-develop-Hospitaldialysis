using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Repository.Core.Interfaces.Base;
using Wasenshi.HemoDialysisPro.Repository.Interfaces;

namespace Wasenshi.HemoDialysisPro.Repositories.Repository
{
    public class PatientHistoryRepository : RepositoryBase<PatientHistory>, IPatientHistoryRepository
    {
        public PatientHistoryRepository(IContextAdapter context) : base(context)
        {
        }

        protected override IQueryable<PatientHistory> GetQueryWithIncludes()
        {
            return base.GetQueryWithIncludes()
                .Include(x => x.HistoryItem)
                .Include(x => x.Patient);
        }

        public void CreateOrUpdateBatch(IEnumerable<PatientHistory> entries)
        {
            foreach (var item in entries)
            {
                var existing = context.PatientHistories.AsNoTracking().FirstOrDefault(x => x.HistoryItemId == item.HistoryItemId && x.PatientId == item.PatientId);
                if (existing != null)
                {
                    context.PatientHistories.Update(item);
                }
                else
                {
                    context.PatientHistories.Add(item);
                }
            }
        }
    }
}
