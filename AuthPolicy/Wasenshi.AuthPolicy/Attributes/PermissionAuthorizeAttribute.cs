using Microsoft.AspNetCore.Mvc.Filters;
using System;

namespace Wasenshi.AuthPolicy.Attributes
{
    /// <summary>
    /// Use this to mark the action or the controller to check for specific claim-based permission.
    /// (Check whether current user has specified permission or not)
    /// <br></br>
    /// Permission on action itself will have the highest priority, and get checked first. 
    /// <br></br>
    /// <br></br>
    /// Note: Multiple permission attributes on the same action will result in "and" logic.
    /// <br></br>
    /// Use "," for multiple alternate permissions on the same attribute instead ("or" logic)
    /// <br></br>
    /// <br></br>
    /// Note:
    /// Priority permission: if set on controller will result in "or" logic where it act as an optional alternate permission that will bypass all others,
    /// <br></br>
    /// if set on the action/method itself will result in "and" logic where it act as a required permission, the absence of it will absolutely be forbidden.
    /// <br></br>
    /// <br></br>
    /// Note: Global permission will automatically bypass any other permission requirement.
    /// <br></br>
    /// <br></br>
    /// Warning: The permission name cannot contain "_" (underscore) nor "," (comma), as it will result in unexpected or faulty behavior.
    /// (Use "-" instead)
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
    public class PermissionAuthorizeAttribute : Attribute, IFilterMetadata
    {
        public PermissionAuthorizeAttribute(string permissionName, bool priority = false)
        {
            PermissionName = permissionName;
            Priority = priority;
        }
        public string PermissionName { get; }
        public bool Priority { get; }
    }
}
