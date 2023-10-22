using System;
using Wasenshi.AuthPolicy.Options;
using Wasenshi.AuthPolicy.Policies;

namespace Wasenshi.AuthPolicy
{
    public static class RoleLevelPolicyExtension
    {
        /// <summary>
        /// Add default policy for specific key type with built-in implementation of RoleLevelPolicy. If got added more than one time, the last one that got added will be effective.
        /// </summary>
        /// <typeparam name="TId"></typeparam>
        /// <param name="options"></param>
        public static AuthPolicyOptions AddRoleLevelPolicy<TId>(this AuthPolicyOptions options, Action<RoleLevelPolicySetting<TId>> configuration = null) { return options.AddRoleLevelPolicy(null, configuration); }
        /// <summary>
        /// Add a policy for specific key type with built-in implementation of RoleLevelPolicy. If the same policy name got added more than one time, the last one that got added will be effective.
        /// </summary>
        /// <typeparam name="TId"></typeparam>
        /// <param name="options"></param>
        /// <param name="policyName"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static AuthPolicyOptions AddRoleLevelPolicy<TId>(this AuthPolicyOptions options, string policyName, Action<RoleLevelPolicySetting<TId>> configuration = null)
        {
            Action<IConfigurable<RoleLevelPolicy<TId>>> action = null;
            if (configuration != null)
            {
                action = (p) => configuration((RoleLevelPolicySetting<TId>)p);
            }
            options.AddPolicy<RoleLevelPolicy<TId>, TId>(policyName, action);

            return options;
        }
    }
}
