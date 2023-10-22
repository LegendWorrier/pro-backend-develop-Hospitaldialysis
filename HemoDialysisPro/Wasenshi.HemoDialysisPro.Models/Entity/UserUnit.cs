using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Wasenshi.HemoDialysisPro.Models
{
    public class UserUnit : IEqualityComparer<UserUnit>
    {
        [Key]
        public Guid UserId { get; set; }
        [Key]
        public int UnitId { get; set; }

        [JsonIgnore, NotMapped]
        public Unit Unit { get; set; }

        public bool Equals(UserUnit x, UserUnit y)
        {
            if (x == null || y == null)
            {
                return x == y;
            }
            return x.UserId == y.UserId && x.UnitId == y.UnitId;
        }

        public int GetHashCode([DisallowNull] UserUnit obj)
        {
            return obj.UserId.GetHashCode() ^ obj.UnitId.GetHashCode();
        }
    }
}
