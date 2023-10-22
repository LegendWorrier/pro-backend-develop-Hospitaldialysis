using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Wasenshi.HemoDialysisPro.Models.Interfaces
{
    public interface IUser : IEntityBase<Guid>
    {
        string UserName { get; set; }
        string NormalizedUserName { get; set; }

        string Email { get; set; }
        string PhoneNumber { get; set; }

        string EmployeeId { get; set; }
        string FirstName { get; set; }
        string LastName { get; set; }
        string Signature { get; set; } // File Id

        public bool IsPartTime { get; set; }

        [JsonConverter(typeof(UnitCollectionConverter))]
        ICollection<UserUnit> Units { get; set; }

        [JsonIgnore]
        List<RefreshToken> RefreshTokens { get; set; }
    }
}
