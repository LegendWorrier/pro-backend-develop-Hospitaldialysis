using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Repositories.Interfaces;
using Wasenshi.HemoDialysisPro.Repository.Core.Interfaces.Base;

namespace Wasenshi.HemoDialysisPro.Repositories.Repository
{
    public class ExecutionRecordRepository : Repository<ExecutionRecord, Guid>, IExecutionRecordRepository
    {
        public ExecutionRecordRepository(IContextAdapter context) : base(context)
        {
        }

        protected override IQueryable<ExecutionRecord> GetQueryWithIncludes()
        {
            return base.GetQueryWithIncludes()
                .Include(x => x.Hemodialysis)
                .Include(x => (x as MedicineRecord).Prescription)
                .ThenInclude(x => x.Medicine)
                .AsSingleQuery();
        }

        public IQueryable<MedicineRecord> GetMedicineRecords(bool included = true)
        {
            if (included)
            {
                return context.MedicineRecords.AsNoTracking()
                    .Include(x => x.Hemodialysis)
                    .Include(x => x.Prescription).ThenInclude(x => x.Medicine)
                    .AsSingleQuery();
            }

            return context.MedicineRecords.AsNoTracking();
        }

        public IQueryable<FlushRecord> GetFlushRecords(bool included = true)
        {
            if (included)
            {
                return context.FlushRecords.AsNoTracking()
                    .Include(x => x.Hemodialysis);
            }

            return context.FlushRecords.AsNoTracking();
        }
    }
}
