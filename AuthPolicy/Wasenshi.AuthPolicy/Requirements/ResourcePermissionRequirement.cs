using Microsoft.AspNetCore.Authorization;
using System;

namespace Wasenshi.AuthPolicy.Requirements
{
    public class ResourcePermissionRequirement : IAuthorizationRequirement
    {
        public string PermissionPolicyName { get; set; }
        public IServiceProvider Services { get; set; }
        public AuthPolicyOptions Options { get; set; }
    }
}
