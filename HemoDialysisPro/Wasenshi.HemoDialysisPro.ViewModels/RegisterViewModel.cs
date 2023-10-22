using System.Collections.Generic;

namespace Wasenshi.HemoDialysisPro.ViewModels
{
    public class RegisterViewModel
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string EmployeeId { get; set; }

        public bool IsPartTime { get; set; }

        public string Role { get; set; }
        public bool isAdmin { get; set; }

        public ICollection<int> Units { get; set; }

        public string Signature { get; set; }
    }
}
