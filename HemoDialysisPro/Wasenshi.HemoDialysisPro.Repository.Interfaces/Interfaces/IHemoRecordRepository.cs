using System;
using System.Linq;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Repositories.Interfaces.Base;

namespace Wasenshi.HemoDialysisPro.Repositories.Interfaces
{
    public interface IHemoRecordRepository : IRepository<HemodialysisRecord, Guid>
    {
        IQueryable<HemoRecordResult> GetAllWithPatient(bool includePrescription = true);
        IQueryable<HemodialysisRecord> GetAllWithNote(bool includePrescription = true);
    }
}
