using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Repositories.Interfaces.Base;

namespace Wasenshi.HemoDialysisPro.Repositories.Interfaces
{
    public interface ILabUnitOfWork : IUnitOfWork
    {
        ILabExamRepository LabExam { get; }

        IHemoRecordRepository HemoRecord { get; }

        IDialysisPrescriptionRepository Prescription { get; }

        IRepository<LabExamItem, int> LabMaster { get; }
    }
}
