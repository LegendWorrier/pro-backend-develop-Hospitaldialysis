using Microsoft.AspNetCore.Authorization;
using System;
using System.Threading.Tasks;
using Wasenshi.AuthPolicy.AuthHandlers;
using Wasenshi.AuthPolicy.Requirements;

namespace Wasenshi.AuthPolicy.Test
{
    public class TestModel
    {
        public string id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class TestOwnerHandler : ResourcePermissionHandler<TestModel, string>
    {
        public bool isPass = true;

        public TestOwnerHandler()
        {
        }
        public TestOwnerHandler(bool isPass = true)
        {
            this.isPass = isPass;
        }

        protected override string ResolveOwnerId(TestModel resource, IServiceProvider services)
        {
            return resource.id;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ResourcePermissionRequirement requirement, TestModel resource)
        {
            //for Moq setup
            Intercept(context, requirement, resource);
            //for normal monitoring setup
            _callback?.Invoke(context, requirement, resource);

            if (isPass)
            {
                context.Succeed(requirement);
            }
            else
            {
                context.Fail();
            }

            return Task.CompletedTask;
        }

        Action<AuthorizationHandlerContext, ResourcePermissionRequirement, TestModel> _callback;
        public void SetupCallback(Action<AuthorizationHandlerContext, ResourcePermissionRequirement, TestModel> callback)
        {
            _callback = callback;
        }

        public virtual void Intercept(AuthorizationHandlerContext context, ResourcePermissionRequirement requirement, TestModel resource) { }
    }
}
