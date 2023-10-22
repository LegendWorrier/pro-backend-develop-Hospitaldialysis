using Microsoft.AspNetCore.Identity;
using System;
using Wasenshi.HemoDialysisPro.Models.Interfaces;

namespace Wasenshi.HemoDialysisPro.Models
{
    public class Role : IdentityRole<Guid>, IRole
    {
        public Role()
        {
        }
        public Role(string roleName) : base(roleName)
        {
            NormalizedName = roleName.ToUpper();
        }

        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}
