using Microsoft.Extensions.Options;
using System;
using Wasenshi.AuthPolicy.Options;

namespace Wasenshi.AuthPolicy
{
    internal class ConfigurePolicy : IConfigureOptions<AuthPolicyOptions>
    {
        private readonly IServiceProvider _provider;

        public ConfigurePolicy(IServiceProvider provider)
        {
            _provider = provider;
        }

        public void Configure(AuthPolicyOptions options)
        {
            options.RegisterAllPolicies(_provider);
        }
    }
}
