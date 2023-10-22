using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Repositories.Interfaces;
using Wasenshi.HemoDialysisPro.Repository.Core.Interfaces.Base;

namespace Wasenshi.HemoDialysisPro.Repositories.Repository
{
    public class AdmissionRepository : Repository<Admission, Guid>, IAdmissionRepository
    {
        public AdmissionRepository(IContextAdapter context) : base(context)
        {
        }

        protected override IQueryable<Admission> GetQueryWithIncludes()
        {
            return base.GetQueryWithIncludes()
                .Include(x => x.Underlying)
                .ThenInclude(x => x.Underlying)
                .AsSingleQuery();
        }

        public IQueryable<AdmissionUnderlying> Underlyings => context.AdmissionUnderlyings.Include(x => x.Underlying).AsNoTracking();
    }
}
