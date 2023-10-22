using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Repositories.Interfaces.Base;

namespace Wasenshi.HemoDialysisPro.Repositories.Interfaces
{
    public interface IScheduleUnitOfWork : IUnitOfWork
    {
        IScheduleRepository Schedule { get; }

        IScheduleSectionRepository Section { get; }
        IRepository<TempSection, int> TempSection { get; }

        IShiftMetaRepository ShiftMeta { get; }
        IRepository<ScheduleMeta, long> ScheduleMeta { get; }
    }
}
