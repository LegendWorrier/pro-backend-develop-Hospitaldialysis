using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Repositories.Interfaces;
using Wasenshi.HemoDialysisPro.Repository.Core.Interfaces.Base;

namespace Wasenshi.HemoDialysisPro.Repositories.Repository
{
    public class LabExamRepository : Repository<LabExam, Guid>, ILabExamRepository
    {
        public LabExamRepository(IContextAdapter context) : base(context)
        {
        }

        protected override IQueryable<LabExam> GetQueryWithIncludes()
        {
            return base.GetQueryWithIncludes()
                .Include(x => x.LabItem)
                .Include(x => x.Patient);
        }

        public void CreateBatch(IEnumerable<LabExam> labExams)
        {
            context.LabExams.AddRange(labExams);
        }
    }
}
