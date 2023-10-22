using System;
using System.Linq;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Repositories.Interfaces.Base;

namespace Wasenshi.HemoDialysisPro.Repositories.Interfaces
{
    public interface IExecutionRecordRepository : IRepository<ExecutionRecord, Guid>
    {
        IQueryable<MedicineRecord> GetMedicineRecords(bool included = true);
        IQueryable<FlushRecord> GetFlushRecords(bool included = true);
    }
}
