using Wasenshi.HemoDialysisPro.Repositories.Interfaces;
using Wasenshi.HemoDialysisPro.Repositories.Repository;
using Wasenshi.HemoDialysisPro.Repository.Core.Interfaces.Base;

namespace Wasenshi.HemoDialysisPro.Repositories
{
    public class HemoUnitOfWork : IHemoUnitOfWork
    {
        private readonly IContextAdapter _context;

        public IPatientRepository Patient { get; }
        public IHemoRecordRepository HemoRecord { get; }
        public IDialysisPrescriptionRepository Prescription { get; }

        public IExecutionRecordRepository ExecutionRecord { get; }

        public HemoUnitOfWork(IContextAdapter context)
        {
            _context = context;
            Patient = new PatientRepository(context);
            HemoRecord = new HemoRecordRepository(context);
            Prescription = new DialysisPrescriptionRepository(context);
            ExecutionRecord = new ExecutionRecordRepository(context);
        }

        public int Complete()
        {
            return _context.Context.SaveChanges();
        }
    }
}
