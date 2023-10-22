using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Repositories.Interfaces;
using Wasenshi.HemoDialysisPro.Repository.Core.Interfaces.Base;

namespace Wasenshi.HemoDialysisPro.Repositories.Repository
{
    public class ScheduleRepository : Repository<Schedule, Guid>, IScheduleRepository
    {
        public ScheduleRepository(IContextAdapter context) : base(context)
        {
        }

        protected override IQueryable<Schedule> GetQueryWithIncludes()
        {
            return base.GetQueryWithIncludes()
                .Include(x => x.Patient)
                .Include(x => x.Section);
        }
    }
}
