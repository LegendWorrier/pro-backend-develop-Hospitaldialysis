using Microsoft.EntityFrameworkCore;
using System.Linq;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Repository.Core.Interfaces.Base;

namespace Wasenshi.HemoDialysisPro.Repositories.Repository
{
    public class SectionSlotPatientRepository : RepositoryBase<SectionSlotPatient>, ISectionSlotPatientRepository
    {
        public SectionSlotPatientRepository(IContextAdapter context) : base(context)
        {
        }

        protected override IQueryable<SectionSlotPatient> GetQueryWithIncludes()
        {
            return base.GetQueryWithIncludes()
                .Include(x => x.Section)
                .Include(x => x.Patient);
        }
    }
}
