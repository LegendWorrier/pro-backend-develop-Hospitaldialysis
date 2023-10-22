using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Wasenshi.AuthPolicy.AuthHandlers;
using Wasenshi.AuthPolicy.Filter;
using Wasenshi.AuthPolicy.Options;
using Wasenshi.AuthPolicy.Policies;
using Wasenshi.AuthPolicy.Providers;

[assembly: InternalsVisibleTo("Wasenshi.AuthPolicy.Test")]
namespace Wasenshi.AuthPolicy
{
    public static class PolicyConfig
    {
        /// <summary>
        /// Add pre-configured authorization policy for this web host.
        /// This will scan the current assembly for any auth handler that implement <see cref="IAuthorizationHandler"/>
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddPolicy(this IServiceCollection services, Action<AuthPolicyOptions> options = null)
        {
            return services.AddPolicy(options, Assembly.GetCallingAssembly());
        }

        /// <summary>Add pre-configured authorization policy for this web host.
        /// Also specify additional auth handlers for specific resource handling using marker types. The types must implement <see cref="IAuthorizationHandler"/> or be in the assemblies containing <see cref="IAuthorizationHandler"/> class(es)</summary>
        /// <param name="app">The application.</param>
        /// <param name="logger"></param>
        /// <param name="typesFromAssembliesContainingExceptionConfigs"></param>
        /// <returns></returns>
        public static IServiceCollection AddPolicy(this IServiceCollection services, params Type[] typesFromAssembliesContainingExceptionConfigs)
        {
            var assemblies = typesFromAssembliesContainingExceptionConfigs.Select(t => t.GetTypeInfo().Assembly);
            return services.AddPolicy(null, assemblies.ToArray());
        }

        public static IServiceCollection AddPolicy(this IServiceCollection services, params Assembly[] assembliesToScan)
        {
            return services.AddPolicy(null, assembliesToScan);
        }

        /// <summary>
        /// Add pre-configured authorization policy for this web host. Also specify additional auth handlers for specific resource handling using assemblies that contain <see cref="IAuthorizationHandler"/> class(es)
        /// </summary>
        /// <param name="services"></param>
        /// <param name="assembliesToScan"></param>
        /// <returns></returns>
        internal static IServiceCollection AddPolicy(this IServiceCollection services, Action<AuthPolicyOptions> configure, params Assembly[] assembliesToScan)
        {
            services.AddHttpContextAccessor();

            // Add default implementation
            services.Configure<AuthPolicyOptions>(x =>
            {
                x.RoleLevelTableSetting
                .SetRole("PowerAdmin", 2)
                .SetRole("Admin", 1)
                .SetRole("User", 0);

                x.AddRoleLevelPolicy<string>()
                .RegisterDefaultUserConfig();
            });
            // Add user configuration
            if (configure != null)
            {
                services.Configure(configure);
            }
            //Get service provider intermediately to access the options
            var provider = services.BuildServiceProvider();
            var options = provider.GetService<IOptions<AuthPolicyOptions>>().Value;
            //Register Role Level Table
            services.AddSingleton(new RoleLevelTable(options.RoleLevelTableSetting.GetImmutableTable()));
            //Register all policies
            services.ConfigureOptions<ConfigurePolicy>();

            //Add Auth Handlers

            var optionAssemblies = options.HandlerList ?? Enumerable.Empty<Assembly>();
            var allTypes = assembliesToScan.Concat(optionAssemblies).Where(a => !a.IsDynamic).SelectMany(a => a.DefinedTypes).ToArray();
            foreach (var type in allTypes)
            {
                if (typeof(IAuthorizationHandler).IsAssignableFrom(type) && !type.IsAbstract)
                {
                    services.AddTransient(typeof(IAuthorizationHandler), type);
                }
            }

            services.AddTransient<IAuthorizationHandler, ResourcePermissionHandlerDefault>();
            services.AddTransient<IAuthorizationHandler, FieldEditPermissionHandler>();
            services.AddTransient<IAuthorizationHandler, PermissionAuthorizationHandler>();

            services.AddSingleton<IAuthorizationPolicyProvider, AuthPolicyProvider>();
            services.AddSingleton<IFilterProvider, AuthFilterProvider>();

            services.Configure<MvcOptions>(o => o.Filters.Add<AuthorizationFilter>());

            return services;
        }
    }
}
