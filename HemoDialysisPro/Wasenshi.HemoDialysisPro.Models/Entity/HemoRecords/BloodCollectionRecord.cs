using Microsoft.EntityFrameworkCore;

namespace Wasenshi.HemoDialysisPro.Models
{
    [Owned]
    public class BloodCollectionRecord
    {
        public string Pre { get; set; }
        public string Post { get; set; }
    }
}