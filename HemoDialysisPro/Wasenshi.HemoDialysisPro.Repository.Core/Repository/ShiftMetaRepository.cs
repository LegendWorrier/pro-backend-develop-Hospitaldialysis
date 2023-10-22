using Microsoft.EntityFrameworkCore;
using System.Linq;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Repositories.Interfaces;
using Wasenshi.HemoDialysisPro.Repository.Core.Interfaces.Base;

namespace Wasenshi.HemoDialysisPro.Repositories.Repository
{
    public class ShiftMetaRepository : Repository<ShiftMeta, long>, IShiftMetaRepository
    {
        public ShiftMetaRepository(IContextAdapter context) : base(context)
        {
        }

        protected override IQueryable<ShiftMeta> GetQueryWithIncludes()
        {
            return base.GetQueryWithIncludes()
                .Include(x => x.ScheduleMeta);
        }
    }
}
