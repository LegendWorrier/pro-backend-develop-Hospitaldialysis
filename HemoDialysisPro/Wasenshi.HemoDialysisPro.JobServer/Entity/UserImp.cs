using System.ComponentModel.DataAnnotations;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Models.Interfaces;

namespace Wasenshi.HemoDialysisPro.JobsServer
{
    public class UserImp : IUser
    {
        [Key]
        public Guid Id { get; set; }
        public string UserName { get; set; }
        public string NormalizedUserName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }

        public string PasswordHash { get; set; }
        public string EmployeeId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Signature { get; set; }

        public bool IsPartTime { get; set; }

        public ICollection<UserUnit> Units { get; set; }

        public List<RefreshToken> RefreshTokens { get; set; }


        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}
