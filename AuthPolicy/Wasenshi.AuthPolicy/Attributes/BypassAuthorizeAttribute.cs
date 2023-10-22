using System;

namespace Wasenshi.AuthPolicy.Attributes
{
    /// <summary>
    /// Decorate this to an Action or a parameter of any action to make it bypass authorization on each fields of the model or any permission check.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public class BypassAuthorizeAttribute : Attribute
    {
    }
}
