using System.Collections.Generic;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Repositories.Interfaces.Base;

namespace Wasenshi.HemoDialysisPro.Repository.Interfaces
{
    public interface IPatientHistoryRepository: IRepositoryBase<PatientHistory>
    {
        void CreateOrUpdateBatch(IEnumerable<PatientHistory> entries);
    }
}