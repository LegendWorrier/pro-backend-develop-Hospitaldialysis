using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Repositories.Interfaces.Base;

namespace Wasenshi.HemoDialysisPro.Repositories.Interfaces
{
    public interface IAssessmentUnitOfWork : IUnitOfWork
    {
        IAssessmentRepository Assessments { get; }
        IRepository<AssessmentOption, long> Options { get; }
        IRepository<AssessmentGroup, int> Groups { get; }

        IAssessmentItemRepository AssessmentsItems { get; }
    }
}
