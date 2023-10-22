using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Services.Interfaces.Base;

namespace Wasenshi.HemoDialysisPro.Services.Interfaces
{
    public interface ISystemBoundLabProcessor : IApplicationService
    {
        void ProcessBUN(HemodialysisRecord hemosheet);
        void ProcessBUN(LabExam item, bool forceUpdateCheck = false);
        void CleanBUNCalculation(HemodialysisRecord hemosheet);
        void CleanBUNCalculation(LabExam item);

        void Commit();
    }
}