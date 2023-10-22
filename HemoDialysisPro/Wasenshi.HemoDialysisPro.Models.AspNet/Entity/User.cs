using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Wasenshi.HemoDialysisPro.Models.Interfaces;

namespace Wasenshi.HemoDialysisPro.Models
{
    public class User : IdentityUser<Guid>, IUser
    {
        public string EmployeeId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Signature { get; set; } // File Id

        public bool IsPartTime { get; set; } = false;

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
