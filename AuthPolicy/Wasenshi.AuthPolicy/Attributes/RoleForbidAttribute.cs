using System;

namespace Wasenshi.AuthPolicy.Attributes
{
    /// <summary>
    /// Forbid any user with specified role from editing this data
    /// Note: Restrict takes precedence over Forbid. (If RoleRestrict is specified, then RoleForbid is nullified)
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class RoleForbidAttribute : Attribute
    {
        public RoleForbidAttribute(params string[] role)
        {
            Role = role;
        }

        public string[] Role { get; }
    }
}
