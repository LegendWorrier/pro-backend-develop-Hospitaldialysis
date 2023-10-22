using Wasenshi.HemoDialysisPro.Models;

namespace Wasenshi.HemoDialysisPro.Web.Api.Utils.ModelSearchers
{
    public class LabOverviewSearch : PatientBaseSearcher<LabOverview>
    {
        public LabOverviewSearch() : base((LabOverview l) => l.Patient)
        {
        }
    }
}
