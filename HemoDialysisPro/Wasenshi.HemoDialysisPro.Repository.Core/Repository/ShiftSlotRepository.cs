using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Repositories.Interfaces;
using Wasenshi.HemoDialysisPro.Repository.Core.Interfaces.Base;

namespace Wasenshi.HemoDialysisPro.Repositories.Repository
{
    public class ShiftSlotRepository : Repository<ShiftSlot, Guid>, IShiftSlotRepository
    {
        public ShiftSlotRepository(IContextAdapter context) : base(context)
        {
        }

        protected override IQueryable<ShiftSlot> GetQueryWithIncludes()
        {
            return base.GetQueryWithIncludes()
                .Include(x => x.ShiftMeta)
                .ThenInclude(x => x.ScheduleMeta);
        }
    }
}
