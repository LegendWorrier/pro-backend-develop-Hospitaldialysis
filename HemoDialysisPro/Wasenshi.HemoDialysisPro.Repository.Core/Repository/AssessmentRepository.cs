using Microsoft.EntityFrameworkCore;
using System.Linq;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Repositories.Interfaces;
using Wasenshi.HemoDialysisPro.Repository.Core.Interfaces.Base;

namespace Wasenshi.HemoDialysisPro.Repositories.Repository
{
    public class AssessmentRepository : Repository<Assessment, long>, IAssessmentRepository
    {
        public AssessmentRepository(IContextAdapter context) : base(context)
        {
        }

        protected override IQueryable<Assessment> GetQueryWithIncludes()
        {
            return base.GetQueryWithIncludes()
                .Include(x => x.OptionsList);
        }
    }
}
