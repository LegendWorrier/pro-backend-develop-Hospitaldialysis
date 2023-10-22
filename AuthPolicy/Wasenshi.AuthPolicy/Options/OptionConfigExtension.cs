using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Wasenshi.AuthPolicy.Options
{
    public static class OptionConfigExtension
    {
        //=================================== User Config =======================================
        public static AuthPolicyOptions RegisterUserConfig<TUser, TId>(this AuthPolicyOptions options) where TUser : IUserConfig<TId>
        {
            string keyName = GetUserConfigKey<TId>();
            Type configType = typeof(TUser);
            if (!options.UserConfigs.TryAdd(keyName, configType))
            {
                throw new InvalidOperationException($"The user configuration for type {typeof(IUserConfig<>).Name}<{typeof(TId).Name}> has already been registered.");
            }
            return options;
        }

        internal static TUserConfig GetUserConfig<TUserConfig, TId>(this AuthPolicyOptions options, IServiceProvider services) where TUserConfig : IUserConfig<TId>
        {
            string keyName = GetUserConfigKey<TId>();
            if (!options.UserConfigs.ContainsKey(keyName))
            {
                throw new InvalidOperationException($"The user configuration for type {typeof(IUserConfig<>).Name}<{typeof(TId).Name}> hasn't been registered yet.");
            }
            Type configType = options.UserConfigs[keyName];
            var userConfig = (TUserConfig)CreateInstance(services, configType);
            return userConfig;
        }

        private static string GetUserConfigKey<T>() => $"{typeof(IUserConfig<>).FullName}-{typeof(T).Name}";

        internal static void RegisterDefaultUserConfig(this AuthPolicyOptions options)
        {
            options.RegisterUserConfig<DefaultUserConfig, string>();
        }

        //=================================== Auth Handler =============================================

        public static void AddHandler(this AuthPolicyOptions options, params Type[] typesFromAssembliesContainingExceptionConfigs)
        {
            var assemblies = typesFromAssembliesContainingExceptionConfigs.Select(t => t.GetTypeInfo().Assembly);
            options.AddHandler(assemblies.ToArray());
        }

        public static void AddHandler(this AuthPolicyOptions options, params Assembly[] assembliesToScan)
        {
            options.HandlerList = assembliesToScan;
        }

        //=================================== Auth Policy ==================================================

        /// <summary>
        /// Add default policy for specific key type. If got added more than one time, the last one that got added will be effective.
        /// </summary>
        /// <typeparam name="TPolicy"></typeparam>
        /// <typeparam name="TId"></typeparam>
        /// <param name="options"></param>
        public static void AddPolicy<TPolicy, TId>(this AuthPolicyOptions options, Action<IConfigurable<TPolicy>> configuration = null) where TPolicy : class, IAuthPolicy<TId>
        {
            options.AddPolicy<TPolicy, TId>(null, configuration);
        }
        /// <summary>
        /// Add default policy for specific key type. If got added more than one time, the last one that got added will be effective.
        /// </summary>
        /// <typeparam name="TId"></typeparam>
        /// <param name="options"></param>
        /// <param name="policyInstanceType"></param>
        public static void AddPolicy<TId>(this AuthPolicyOptions options, Type policyInstanceType, Action<object> configuration = null)
        {
            options.AddPolicy<TId>(null, policyInstanceType, configuration);
        }
        /// <summary>
        /// Add new policy by type. Note: All policy is singleton.
        /// </summary>
        /// <typeparam name="TPolicy"></typeparam>
        /// <typeparam name="TId"></typeparam>
        /// <param name="options"></param>
        /// <param name="services"></param>
        public static void AddPolicy<TPolicy, TId>(this AuthPolicyOptions options, string policyName, Action<IConfigurable<TPolicy>> configuration = null) where TPolicy : class, IAuthPolicy<TId>
        {
            Action<object> action = null;
            if (configuration != null)
            {
                action = (p) => configuration((IConfigurable<TPolicy>)p);
            }
            options.AddPolicy<TId>(policyName, typeof(TPolicy), action);
        }
        /// <summary>
        /// Add new policy by type. Note: All policy is singleton.
        /// </summary>
        /// <typeparam name="TId"></typeparam>
        /// <param name="options"></param>
        /// <param name="policyName"></param>
        /// <param name="policyInstanceType"></param>
        public static void AddPolicy<TId>(this AuthPolicyOptions options, string policyName, Type policyInstanceType, Action<object> configuration)
        {
            policyName = policyName?.ToUpper();
            string keyName = GetPolicyKey<TId>(policyName);
            if (options.AuthPolicyKeys.Contains(keyName) && options.AuthPolicieTypes.ContainsKey(keyName))
            {
                options.AuthPolicieTypes[keyName] = policyInstanceType;
            }
            else
            {
                options.AuthPolicieTypes.Add(keyName, policyInstanceType);
            }

            if (configuration != null && !options.AuthPoliciesConfigs.TryAdd(keyName, configuration))
            {
                options.AuthPoliciesConfigs[keyName] = configuration;
            }
        }
        /// <summary>
        /// Add new policy by object instance. Note: All policy is singleton.
        /// </summary>
        /// <typeparam name="TId"></typeparam>
        /// <param name="options"></param>
        /// <param name="policyName"></param>
        /// <param name="policy"></param>
        public static void AddPolicy<TId>(this AuthPolicyOptions options, string policyName, IAuthPolicy<TId> policy, Action<IConfigurable<IAuthPolicy<TId>>> configuration)
        {
            policyName = policyName?.ToUpper();
            string keyName = GetPolicyKey<TId>(policyName);
            if (options.AuthPolicyKeys.Contains(keyName) && options.AuthPolicies.ContainsKey(keyName))
            {
                options.AuthPolicies[keyName] = policy;
            }
            else
            {
                options.AuthPolicies.Add(keyName, policy);
            }

            if (configuration != null)
            {
                void convertAction(object c) => configuration(c as IConfigurable<IAuthPolicy<TId>>);
                if (!options.AuthPoliciesConfigs.TryAdd(keyName, convertAction)) options.AuthPoliciesConfigs[keyName] = convertAction;
            }
        }

        private static string GetPolicyKey<T>(string policyName) => $"{typeof(T).FullName}-{policyName}";

        internal static void RegisterAllPolicies(this AuthPolicyOptions options, IServiceProvider provider)
        {
            var allPolicies = options.AuthPolicieTypes
                .Select(x =>
                new KeyValuePair<string, object>(x.Key, ConfigurePolicy(provider, x.Key, x.Value, options)))
                .Concat(options.AuthPolicies
                .Select(x =>
                new KeyValuePair<string, object>(x.Key, x.Value)
                ));

            options.AuthPolicies = new Dictionary<string, object>(allPolicies);
        }

        private static object ConfigurePolicy(IServiceProvider provider, string key, Type policyType, AuthPolicyOptions option)
        {
            var instance = provider.CreateInstance(policyType);
            if (option.AuthPoliciesConfigs.ContainsKey(key))
            {
                option.AuthPoliciesConfigs[key](instance);
            }
            return instance;
        }

        internal static IAuthPolicy<TId> GetAuthPolicy<TId>(this AuthPolicyOptions options, string policyName)
        {
            var keyName = GetPolicyKey<TId>(policyName);
            return (IAuthPolicy<TId>)options.AuthPolicies[keyName];
        }

        //=================================== Auth Roles ==================================================
        public static void AddRoles(this AuthPolicyOptions options)
        {
            throw new NotImplementedException();
        }

        //=================================================================================================
        internal static object CreateInstance(this IServiceProvider services, Type instanceType, params object[] parameter)
        {
            return ActivatorUtilities.CreateInstance(services, instanceType, parameter);
        }
    }
}
