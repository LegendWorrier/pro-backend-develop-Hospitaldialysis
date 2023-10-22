using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Repositories.Interfaces.Base;

namespace Wasenshi.HemoDialysisPro.Repositories.Interfaces
{
    public interface IShiftUnitOfWork : IUnitOfWork
    {
        IScheduleSectionRepository Section { get; }

        IShiftMetaRepository ShiftMeta { get; }
        IShiftSlotRepository ShiftSlot { get; }
        IRepository<UserShift, long> UserShift { get; }
        IRepository<ScheduleMeta, long> ScheduleMeta { get; }
        IShiftInchargeRepository ShiftIncharge { get; }
    }
}
