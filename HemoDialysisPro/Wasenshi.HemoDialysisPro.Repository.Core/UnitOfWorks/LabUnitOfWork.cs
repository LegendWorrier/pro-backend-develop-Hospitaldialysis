using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Repositories.Interfaces;
using Wasenshi.HemoDialysisPro.Repositories.Interfaces.Base;
using Wasenshi.HemoDialysisPro.Repositories.Repository;
using Wasenshi.HemoDialysisPro.Repository.Core.Interfaces.Base;

namespace Wasenshi.HemoDialysisPro.Repositories
{
    public class LabUnitOfWork : ILabUnitOfWork
    {
        private readonly IContextAdapter _context;

        public IHemoRecordRepository HemoRecord { get; }
        public IDialysisPrescriptionRepository Prescription { get; }

        public ILabExamRepository LabExam { get; }
        public IRepository<LabExamItem, int> LabMaster { get; }

        public LabUnitOfWork(IContextAdapter context)
        {
            _context = context;
            LabExam = new LabExamRepository(context);
            HemoRecord = new HemoRecordRepository(context);
            Prescription = new DialysisPrescriptionRepository(context);
            LabMaster = new Repository<LabExamItem, int>(context);
        }

        public int Complete()
        {
            return _context.Context.SaveChanges();
        }
    }
}
