using System;
using System.Collections.Generic;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Repositories.Interfaces.Base;

namespace Wasenshi.HemoDialysisPro.Repositories.Interfaces
{
    public interface ILabExamRepository : IRepository<LabExam, Guid>
    {
        void CreateBatch(IEnumerable<LabExam> labExams);
    }
}