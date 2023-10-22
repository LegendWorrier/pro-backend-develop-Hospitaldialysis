using Microsoft.EntityFrameworkCore;

namespace Wasenshi.HemoDialysisPro.Models
{
    [Owned]
    public class EmergencyContact
    {
        public string Name { get; set; }
        public string PhoneNumber { get; set; }
        public string Relationship { get; set; }
    }
}