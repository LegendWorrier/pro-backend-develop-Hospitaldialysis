using System;

namespace Wasenshi.AuthPolicy.Attributes
{
    /// <summary>
    /// Use this together with <see cref="AuthPolicyExtension.ValidateResourcePermissionAsync{T}"/> of the framework to validate permission of the user on any resource with specific AuthPolicy.
    /// <br></br>
    /// <br></br>Note: Leave policyName empty for default policy.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
    public class ResourcePermissionPolicyAttribute : Attribute
    {
        public ResourcePermissionPolicyAttribute(string policyName)
        {
            PolicyName = policyName;
        }

        public string PolicyName { get; }
    }
}
