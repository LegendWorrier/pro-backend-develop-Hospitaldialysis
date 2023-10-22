using Wasenshi.HemoDialysisPro.Repositories.Interfaces;
using Wasenshi.HemoDialysisPro.Repositories.Repository;
using Wasenshi.HemoDialysisPro.Repository.Core.Interfaces.Base;

namespace Wasenshi.HemoDialysisPro.Repositories
{
    public class PatientUnitOfWork : IPatientUnitOfWork
    {
        private readonly IContextAdapter _context;

        public IPatientRepository Patient { get; }
        public IScheduleRepository Schedule { get; }
        public ISectionSlotPatientRepository Slot { get; }
        public IHemoRecordRepository HemoRecord { get; }
        public IMedicinePrescriptionRepository MedPres { get; }

        public PatientUnitOfWork(IContextAdapter context)
        {
            _context = context;
            Patient = new PatientRepository(context);
            Schedule = new ScheduleRepository(context);
            Slot = new SectionSlotPatientRepository(context);
            HemoRecord = new HemoRecordRepository(context);
            MedPres = new MedicinePrescriptionRepository(context);
        }

        public int Complete()
        {
            return _context.Context.SaveChanges();
        }
    }
}
