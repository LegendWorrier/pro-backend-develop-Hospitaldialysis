using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using Wasenshi.AuthPolicy.Attributes;
using Wasenshi.AuthPolicy.Filter;
using Wasenshi.AuthPolicy.Utillities;

namespace Wasenshi.AuthPolicy.Providers
{
    internal class AuthFilterProvider : IFilterProvider
    {
        private readonly ConcurrentHashSet<string> _cache = new ConcurrentHashSet<string>();

        public AuthFilterProvider()
        {
        }

        public int Order => 0;

        public void OnProvidersExecuted(FilterProviderContext context)
        {
            var action = context.ActionContext.ActionDescriptor as ControllerActionDescriptor;
            if (_cache.Contains($"{action.ControllerName}-{action.ActionName}"))
            {
                return;
            }

            var fieldAuth = action.FilterDescriptors.FirstOrDefault(x => x.Filter is FieldAuthorizeAttribute);
            if (fieldAuth != null)
            {
                var filter = context.Results.First(m => m.Descriptor == fieldAuth);
                filter.Filter = ActivatorUtilities.CreateInstance<FieldEditPermissionFilter>(context.ActionContext.HttpContext.RequestServices);
            }
            var permissionAuth = action.FilterDescriptors.FirstOrDefault(x => x.Filter is  PermissionAuthorizeAttribute);
            if (permissionAuth != null)
            {
                var filter = context.Results.First(m => m.Descriptor == permissionAuth);
                filter.Filter = ActivatorUtilities.CreateInstance<PermissionFilter>(context.ActionContext.HttpContext.RequestServices);
            }

            _cache.Add($"{action.ControllerName}-{action.ActionName}");
        }

        public void OnProvidersExecuting(FilterProviderContext context)
        {
        }
    }
}
