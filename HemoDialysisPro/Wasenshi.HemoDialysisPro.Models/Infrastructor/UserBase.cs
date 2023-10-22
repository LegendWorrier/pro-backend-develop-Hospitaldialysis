using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Wasenshi.HemoDialysisPro.Models.Interfaces;

namespace Wasenshi.HemoDialysisPro.Models.Infrastructor
{
    public class UserBase : IUser
    {
        public Guid Id { get; set; }
        public string UserName { get; set; }
        public string NormalizedUserName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string EmployeeId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Signature { get; set; }

        public bool IsPartTime { get; set; }

        [JsonConverter(typeof(UnitCollectionConverter))]
        public ICollection<UserUnit> Units { get; set; }

        [JsonIgnore]
        public List<RefreshToken> RefreshTokens { get; set; }

        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}
