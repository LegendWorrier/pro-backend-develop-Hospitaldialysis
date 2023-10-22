using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Repositories.Interfaces;
using Wasenshi.HemoDialysisPro.Repository.Core.Interfaces.Base;

namespace Wasenshi.HemoDialysisPro.Repositories.Repository
{
    public class DialysisPrescriptionRepository : Repository<DialysisPrescription, Guid>, IDialysisPrescriptionRepository
    {
        public DialysisPrescriptionRepository(IContextAdapter context) : base(context)
        {
        }

        protected override IQueryable<DialysisPrescription> GetQueryWithIncludes()
        {
            return base.GetQueryWithIncludes()
                .Include(x => x.HemodialysisRecords)
                .AsSingleQuery();
        }
    }
}
