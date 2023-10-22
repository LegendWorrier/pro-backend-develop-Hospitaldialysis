using System.Collections.Generic;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Repositories.Interfaces.Base;

namespace Wasenshi.HemoDialysisPro.Repositories.Interfaces
{
    public interface IScheduleSectionRepository : IRepository<ScheduleSection, int>
    {
        void BulkInsertOrUpdate(IEnumerable<ScheduleSection> sections);
        void BulkDelete(IEnumerable<ScheduleSection> sections);
    }
}
