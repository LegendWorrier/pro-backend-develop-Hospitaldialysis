using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Repositories.Interfaces;
using Wasenshi.HemoDialysisPro.Repositories.Interfaces.Base;
using Wasenshi.HemoDialysisPro.Repositories.Repository;
using Wasenshi.HemoDialysisPro.Repository.Core.Interfaces.Base;

namespace Wasenshi.HemoDialysisPro.Repositories.UnitOfWorks
{
    public class ScheduleUnitOfWork : IScheduleUnitOfWork
    {
        private readonly IContextAdapter _context;

        public IScheduleRepository Schedule { get; }
        public IScheduleSectionRepository Section { get; }

        public IShiftMetaRepository ShiftMeta { get; }

        public IRepository<ScheduleMeta, long> ScheduleMeta { get; }

        public IRepository<TempSection, int> TempSection { get; }

        public ScheduleUnitOfWork(IContextAdapter context)
        {
            _context = context;
            Schedule = new ScheduleRepository(context);
            Section = new ScheduleSectionRepository(context);
            ShiftMeta = new ShiftMetaRepository(context);
            ScheduleMeta = new Repository<ScheduleMeta, long>(context);
            TempSection = new Repository<TempSection, int>(context);
        }

        public int Complete()
        {
            return _context.Context.SaveChanges();
        }
    }
}
