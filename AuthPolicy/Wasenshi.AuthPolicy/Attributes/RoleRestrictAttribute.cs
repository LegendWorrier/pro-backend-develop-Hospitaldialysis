using System;

namespace Wasenshi.AuthPolicy.Attributes
{
    /// <summary>
    /// Restrict the data. Only user with specified role(s) can edit this data.
    /// Note: Restrict takes precedence over Forbid.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class RoleRestrictAttribute : Attribute
    {
        public RoleRestrictAttribute(params string[] role)
        {
            Role = role;
        }

        public string[] Role { get; }
    }
}
