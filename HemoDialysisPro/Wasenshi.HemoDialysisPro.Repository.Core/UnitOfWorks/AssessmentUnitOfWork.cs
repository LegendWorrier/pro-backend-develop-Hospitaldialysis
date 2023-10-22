using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Repositories.Interfaces;
using Wasenshi.HemoDialysisPro.Repositories.Interfaces.Base;
using Wasenshi.HemoDialysisPro.Repositories.Repository;
using Wasenshi.HemoDialysisPro.Repository.Core.Interfaces.Base;

namespace Wasenshi.HemoDialysisPro.Repositories
{
    public class AssessmentUnitOfWork : IAssessmentUnitOfWork
    {
        private readonly IContextAdapter _context;


        public IAssessmentRepository Assessments { get; }
        /// <summary>
        /// Extra 
        /// </summary>
        public IRepository<AssessmentOption, long> Options { get; }

        public IRepository<AssessmentGroup, int> Groups { get; }

        public IAssessmentItemRepository AssessmentsItems { get; }

        public AssessmentUnitOfWork(IContextAdapter context)
        {
            _context = context;
            Assessments = new AssessmentRepository(context);
            Options = new Repository<AssessmentOption, long>(context);
            Groups = new Repository<AssessmentGroup, int>(context);
            AssessmentsItems = new AssessmentItemRepository(context);
        }

        public int Complete()
        {
            return _context.Context.SaveChanges();
        }
    }
}
