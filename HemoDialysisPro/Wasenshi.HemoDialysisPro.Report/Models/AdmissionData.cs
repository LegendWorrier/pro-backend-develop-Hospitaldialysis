using System.Collections.Generic;
using System.Linq;
using Wasenshi.HemoDialysisPro.Models;

namespace Wasenshi.HemoDialysisPro.Report.Models
{
    public class AdmissionData : Admission
    {
        public ICollection<string> UnderlyingList => Underlying.Select(x => x.Underlying.Name).ToList();
    }
}
