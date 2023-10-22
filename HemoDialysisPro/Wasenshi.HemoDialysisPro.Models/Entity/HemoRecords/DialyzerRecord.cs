using Microsoft.EntityFrameworkCore;

namespace Wasenshi.HemoDialysisPro.Models
{
    [Owned]
    public class DialyzerRecord
    {
        public int UseNo { get; set; }
        public float TCV { get; set; } // %
    }
}