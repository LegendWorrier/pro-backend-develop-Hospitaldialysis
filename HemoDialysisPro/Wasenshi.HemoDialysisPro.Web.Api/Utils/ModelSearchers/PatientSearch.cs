using Wasenshi.HemoDialysisPro.Models;

namespace Wasenshi.HemoDialysisPro.Web.Api.Utils.ModelSearchers
{
    public class PatientSearch : PatientBaseSearcher<Patient>
    {
        public PatientSearch() : base(p => p)
        {
        }
    }
}
