using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;
using Wasenshi.AuthPolicy.Attributes;
using Wasenshi.AuthPolicy.Providers;
using Wasenshi.AuthPolicy.Utillities;

namespace Wasenshi.AuthPolicy
{
    public static class AuthPolicyExtension
    {
        /// <summary>
        /// Use this on a resource to validate it against current user, checking whether the user is accessing his own resource or not. (you need to also implement the auth handler for this resource type.)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="resource"></param>
        public static async Task ValidateResourcePermissionAsync<T>(this ControllerBase controller, T resource) where T : class
        {
            CheckType(resource, nameof(resource));

            var services = controller.HttpContext.RequestServices;
            var auth = services.GetService<IAuthorizationService>();
            var actionDescriptor = controller.ControllerContext.ActionDescriptor;

            List<ResourcePermissionPolicyAttribute> permissionPolicies =
                actionDescriptor.MethodInfo.GetCustomAttributes<ResourcePermissionPolicyAttribute>()
                .Union(actionDescriptor.ControllerTypeInfo.GetCustomAttributes<ResourcePermissionPolicyAttribute>())
                .ToList();
            if (!permissionPolicies.Any())
            {
                permissionPolicies.Add(new ResourcePermissionPolicyAttribute(null));
            }

            foreach (var policy in permissionPolicies)
            {
                string policyName = AuthPolicyProvider.GetPolicyName(AuthPolicyProvider.AuthMode.ResourcePermission, policy?.PolicyName?.ToUpper());
                if (!(await auth.AuthorizeAsync(controller.User, resource, policyName)).Succeeded)
                {
                    throw new UnauthorizedException("Cannot accessing a resource not of your own.");
                }
            }
        }

        /// <summary>
        /// Synchronous version of ValidateOwnerAsync
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="controller"></param>
        /// <param name="resource"></param>
        public static void ValidateResourcePermission<T>(this ControllerBase controller, T resource) where T : class
        {
            controller.ValidateResourcePermissionAsync(resource).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Validate current user against any specified permission(s). Immediately throw Unauthorized Exception if no permission.
        /// </summary>
        public static async Task ValidatePermissionAsync(this ControllerBase controller, params string[] permissions)
        {
            var services = controller.HttpContext.RequestServices;
            var option = services.GetService<IOptionsMonitor<AuthPolicyOptions>>();
            if (controller.User.Claims.Any(x => x.Type == ClaimsPermissionHelper.PERMISSION_TYPE && x.Value == option.CurrentValue.GlobalPermission))
            {
                return;
            }

            if (!await controller.CheckPermissionAsync(permissions))
            {
                throw new UnauthorizedException("No permission for target action.");
            }
        }

        /// <summary>
        /// Check whether current user has any permission(s) specified or not.
        /// </summary>
        public static async Task<bool> CheckPermissionAsync(this ControllerBase controller, params string[] permissions)
        {
            var services = controller.HttpContext.RequestServices;
            var auth = services.GetService<IAuthorizationService>();

            bool hasPermission = false;
            foreach (var permission in permissions)
            {
                string policyName = AuthPolicyProvider.GetPolicyName(AuthPolicyProvider.AuthMode.Permission, permission);
                if ((await auth.AuthorizeAsync(controller.User, policyName)).Succeeded)
                {
                    hasPermission = true;
                    break;
                }
            }

            return hasPermission;
        }

        /// <summary>
        /// Validate current user against any specified permission(s). Immediately throw Unauthorized Exception if no permission.
        /// </summary>
        public static async Task ValidateFieldsAsync<T>(this ControllerBase controller, T model) where T : class
        {
            CheckType(model, nameof(model));

            var services = controller.HttpContext.RequestServices;
            var auth = services.GetService<IAuthorizationService>();

            string policyName = AuthPolicyProvider.GetPolicyName(AuthPolicyProvider.AuthMode.FieldPermission);
            var validation = await auth.AuthorizeAsync(controller.User, model, policyName);
            if (!validation.Succeeded)
            {
                throw new UnauthorizedException("No permission to edit some target fields.");
            }
        }

        //============================================================================================================================

        private static void CheckType(object data, string dataName)
        {
            var type = data.GetType();
            if (type.IsValueType || type == typeof(string))
            {
                throw new InvalidOperationException($"the auth validation can only validate against objects. The {dataName} cannot be a string nor value type.");
            }
        }
    }
}
