using Wasenshi.HemoDialysisPro.Repositories.Interfaces.Base;

namespace Wasenshi.HemoDialysisPro.Repositories.Interfaces
{
    public interface IHemoUnitOfWork : IUnitOfWork
    {
        IPatientRepository Patient { get; }

        IHemoRecordRepository HemoRecord { get; }

        IDialysisPrescriptionRepository Prescription { get; }

        IExecutionRecordRepository ExecutionRecord { get; }
    }
}
