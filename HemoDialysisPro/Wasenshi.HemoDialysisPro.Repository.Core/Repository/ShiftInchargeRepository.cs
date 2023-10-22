using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Repositories.Interfaces;
using Wasenshi.HemoDialysisPro.Repository.Core.Interfaces.Base;

namespace Wasenshi.HemoDialysisPro.Repositories.Repository
{
    public class ShiftInchargeRepository : RepositoryBase<ShiftIncharge>, IShiftInchargeRepository
    {
        private readonly ILogger<ShiftInchargeRepository> logger;

        public ShiftInchargeRepository(IContextAdapter context, ILogger<ShiftInchargeRepository> logger) : base(context)
        {
            this.logger = logger;
        }

        public ShiftIncharge Get(int unitId, DateOnly date, bool include = false)
        {
            return GetAll(include).Where(x => x.UnitId == unitId && x.Date == date).FirstOrDefault();
        }

        private ShiftIncharge GetTrack(int unitId, DateOnly date)
        {
            return _entities.Where(x => x.UnitId == unitId && x.Date == date).FirstOrDefault();
        }

        public EntityEntry<ShiftIncharge> AddOrUpdate(ShiftIncharge entity)
        {
            var old = GetTrack(entity.UnitId, entity.Date);
            if (old != null)
            {
                Delete(old);
            }
            return context.Add(entity);
        }

        public void ClearCollection(ShiftIncharge entity)
        {
            var isAlreadyTracked = _entities.Local.Any(e => e == entity);
            var old = isAlreadyTracked ? entity : _entities.Where(x => x.UnitId == entity.UnitId && x.Date == entity.Date).FirstOrDefault();
            if (old == null)
            {
                return;
            }
            var oldEntry = context.Entry(old);
            // remove range , then detach the parent entity
            var collection = oldEntry.Collection(x => x.Sections).CurrentValue;
            context.RemoveRange(collection);
            oldEntry.State = EntityState.Detached;
        }

        protected override IQueryable<ShiftIncharge> GetQueryWithIncludes()
        {
            return base.GetQueryWithIncludes()
                .Include(x => x.Sections)
                .ThenInclude(x => x.Section);
        }
    }
}
