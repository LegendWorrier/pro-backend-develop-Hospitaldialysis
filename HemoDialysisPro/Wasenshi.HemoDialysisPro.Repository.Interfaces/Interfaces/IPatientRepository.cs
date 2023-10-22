using System.Linq;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Repositories.Interfaces.Base;

namespace Wasenshi.HemoDialysisPro.Repositories.Interfaces
{
    public interface IPatientRepository : IRepository<Patient, string>
    {
        IQueryable<PatientMedicine> Allergies { get; }
        IQueryable<Tag> Tags { get; }
    }
}
