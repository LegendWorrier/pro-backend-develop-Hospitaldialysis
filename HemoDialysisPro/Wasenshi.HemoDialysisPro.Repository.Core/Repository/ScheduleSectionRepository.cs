using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Repositories.Interfaces;
using Wasenshi.HemoDialysisPro.Repository.Core.Interfaces.Base;

namespace Wasenshi.HemoDialysisPro.Repositories.Repository
{
    public class ScheduleSectionRepository : Repository<ScheduleSection, int>, IScheduleSectionRepository
    {
        public ScheduleSectionRepository(IContextAdapter context) : base(context)
        {
        }

        protected override IQueryable<ScheduleSection> GetQueryWithIncludes()
        {
            return base.GetQueryWithIncludes()
                .Include(x => x.Unit);
        }

        public void BulkInsertOrUpdate(IEnumerable<ScheduleSection> sections)
        {
            context.Sections.UpdateRange(sections);
        }

        public void BulkDelete(IEnumerable<ScheduleSection> sections)
        {
            context.Sections.RemoveRange(sections);
        }
    }
}
