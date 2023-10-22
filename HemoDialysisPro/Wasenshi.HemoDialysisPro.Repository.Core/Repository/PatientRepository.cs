using Microsoft.EntityFrameworkCore;
using System.Linq;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Repositories.Interfaces;
using Wasenshi.HemoDialysisPro.Repository.Core.Interfaces.Base;

namespace Wasenshi.HemoDialysisPro.Repositories.Repository
{
    public class PatientRepository : Repository<Patient, string>, IPatientRepository
    {
        public PatientRepository(IContextAdapter context) : base(context)
        {
        }

        protected override IQueryable<Patient> GetQueryWithIncludes()
        {
            return base.GetQueryWithIncludes()
                .Include(x => x.Tags)
                .Include(x => x.Allergy)
                .AsSplitQuery();
        }

        public IQueryable<PatientMedicine> Allergies => context.Allergies.Include(x => x.Medicine).AsNoTracking();

        public IQueryable<Tag> Tags => context.Tags.AsNoTracking();
    }
}
