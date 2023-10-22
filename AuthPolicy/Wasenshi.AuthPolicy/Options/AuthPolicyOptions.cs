using System;
using System.Collections.Generic;
using System.Reflection;
using Wasenshi.AuthPolicy.Policies;

namespace Wasenshi.AuthPolicy
{
    public class AuthPolicyOptions
    {
        public enum MissingHandlerBehavior
        {
            Ignore,
            Warning,
            Error
        }
        /// <summary>
        /// Set whether the error should be thrown or not when there is no auth handler for specific type.
        /// </summary>
        public MissingHandlerBehavior MissingHandler { get; set; } = MissingHandlerBehavior.Error;

        /// <summary>
        /// The permission name for global. This permission will bypass all other permissions. (god mode)
        /// <br></br>
        /// Warning: only use this for something like rootadmin/superadmin. Should not be used for any other purpose.
        /// </summary>
        public string GlobalPermission { get; set; }

        public RoleLevelTableSetting RoleLevelTableSetting { get; } = new RoleLevelTableSetting();

        internal Dictionary<string, Type> UserConfigs = new Dictionary<string, Type>();
        internal Dictionary<string, object> AuthPolicies = new Dictionary<string, object>();

        internal Dictionary<string, Type> AuthPolicieTypes = new Dictionary<string, Type>();
        internal Dictionary<string, Action<object>> AuthPoliciesConfigs = new Dictionary<string, Action<object>>();
        internal HashSet<string> AuthPolicyKeys = new HashSet<string>();

        internal IEnumerable<Assembly> HandlerList;
    }
}
