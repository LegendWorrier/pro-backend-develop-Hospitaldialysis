using Microsoft.Extensions.Logging;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Repositories.Interfaces;
using Wasenshi.HemoDialysisPro.Repositories.Interfaces.Base;
using Wasenshi.HemoDialysisPro.Repositories.Repository;
using Wasenshi.HemoDialysisPro.Repository.Core.Interfaces.Base;

namespace Wasenshi.HemoDialysisPro.Repositories.UnitOfWorks
{
    public class ShiftUnitOfWork : IShiftUnitOfWork
    {
        private readonly IContextAdapter _context;

        public IScheduleSectionRepository Section { get; }

        public IShiftMetaRepository ShiftMeta { get; }

        public IShiftSlotRepository ShiftSlot { get; }

        public IRepository<UserShift, long> UserShift { get; }

        public IRepository<ScheduleMeta, long> ScheduleMeta { get; }

        public IShiftInchargeRepository ShiftIncharge { get; }

        public ShiftUnitOfWork(IContextAdapter context, ILogger<ShiftInchargeRepository> logger)
        {
            _context = context;
            Section = new ScheduleSectionRepository(context);
            ShiftMeta = new ShiftMetaRepository(context);
            ShiftSlot = new ShiftSlotRepository(context);
            UserShift = new Repository<UserShift, long>(context);
            ScheduleMeta = new Repository<ScheduleMeta, long>(context);
            ShiftIncharge = new ShiftInchargeRepository(context, logger);
        }

        public int Complete()
        {
            return _context.Context.SaveChanges();
        }
    }
}
