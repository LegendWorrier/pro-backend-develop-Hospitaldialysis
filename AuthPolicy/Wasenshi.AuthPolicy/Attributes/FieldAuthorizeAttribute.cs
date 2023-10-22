using Microsoft.AspNetCore.Mvc.Filters;
using System;

namespace Wasenshi.AuthPolicy.Attributes
{
    /// <summary>
    /// Use this to mark the action or the controller to check for permission per each fields of model(s).
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public class FieldAuthorizeAttribute : Attribute, IFilterMetadata
    {
    }
}
