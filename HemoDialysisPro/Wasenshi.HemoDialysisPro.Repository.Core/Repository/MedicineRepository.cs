using Microsoft.EntityFrameworkCore;
using System.Linq;
using Wasenshi.HemoDialysisPro.Models.Entity.Stockable;
using Wasenshi.HemoDialysisPro.Repositories.Interfaces;
using Wasenshi.HemoDialysisPro.Repository.Core.Interfaces.Base;

namespace Wasenshi.HemoDialysisPro.Repositories.Repository
{
    public class MedicineRepository : Repository<Medicine>, IMedicineRepository
    {
        public MedicineRepository(IContextAdapter context) : base(context)
        {
        }

        protected override IQueryable<Medicine> GetQueryWithIncludes()
        {
            return base.GetQueryWithIncludes()
                .Include(x => x.Category);
        }
    }
}
