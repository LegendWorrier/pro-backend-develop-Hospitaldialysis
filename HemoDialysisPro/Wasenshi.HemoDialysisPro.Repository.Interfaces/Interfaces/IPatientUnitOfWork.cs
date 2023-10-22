using Wasenshi.HemoDialysisPro.Repositories.Interfaces.Base;
using Wasenshi.HemoDialysisPro.Repositories.Repository;

namespace Wasenshi.HemoDialysisPro.Repositories.Interfaces
{
    public interface IPatientUnitOfWork : IUnitOfWork
    {
        IPatientRepository Patient { get; }
        IScheduleRepository Schedule { get; }
        ISectionSlotPatientRepository Slot { get; }
        IHemoRecordRepository HemoRecord { get; }
        IMedicinePrescriptionRepository MedPres { get; }
    }
}