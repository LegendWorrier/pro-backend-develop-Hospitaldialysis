using System.Collections.Generic;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Services.Interfaces.Base;

namespace Wasenshi.HemoDialysisPro.Services.Interfaces
{
    public interface ILabExamProcessor : IApplicationService
    {
        LabExamResult ProcessData(IEnumerable<LabExam> labExams);
    }
}
