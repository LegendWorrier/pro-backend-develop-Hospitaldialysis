using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Repositories.Interfaces;
using Wasenshi.HemoDialysisPro.Repository.Core.Interfaces.Base;

namespace Wasenshi.HemoDialysisPro.Repositories.Repository
{
    public class MedicinePrescriptionRepository : Repository<MedicinePrescription, Guid>, IMedicinePrescriptionRepository
    {
        public MedicinePrescriptionRepository(IContextAdapter context) : base(context)
        {
        }

        protected override IQueryable<MedicinePrescription> GetQueryWithIncludes()
        {
            return base.GetQueryWithIncludes()
                .Include(x => x.Medicine)
                .Include(x => x.MedicineRecords).ThenInclude(x => x.Hemodialysis)
                .AsSingleQuery();
        }
    }
}
