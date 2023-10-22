using System.Collections.Generic;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Services.Interfaces.Base;

namespace Wasenshi.HemoDialysisPro.Services.Interfaces
{
    public interface IMedHistoryProcessor : IApplicationService
    {
        MedHistoryResult ProcessData(IEnumerable<MedHistoryItem> labExams);
    }
}
