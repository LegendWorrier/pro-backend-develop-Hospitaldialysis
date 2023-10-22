using System;

namespace Wasenshi.AuthPolicy.Attributes
{
    /// <summary>
    /// Restrict the data. Only user with any specified permission(s) can edit this data.
    /// Note: Restrict takes precedence over Forbid.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = false, AllowMultiple = true)]
    public sealed class PermissionRestrictAttribute : Attribute
    {
        public PermissionRestrictAttribute(params string[] permissions)
        {
            Permissions = permissions;
        }

        public string[] Permissions { get; }
    }
}
