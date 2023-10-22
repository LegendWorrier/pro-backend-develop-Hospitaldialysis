using System.Collections.Generic;

namespace Wasenshi.HemoDialysisPro.Models
{
    public class AVResult
    {
        public IEnumerable<AVShunt> AvShunts { get; set; }
        public IEnumerable<AVShuntIssueTreatment> AvShuntIssueTreatments { get; set; }
    }
}
