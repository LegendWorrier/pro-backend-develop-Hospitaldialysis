using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Reflection;
using Wasenshi.HemoDialysisPro.Repositories.Interfaces;
using Wasenshi.HemoDialysisPro.Repositories.Interfaces.Base;
using Wasenshi.HemoDialysisPro.Repositories.Repository;
using Wasenshi.HemoDialysisPro.Repository.Core.Interfaces.Base;

namespace Wasenshi.HemoDialysisPro.Repositories
{
    public static class RegisterHelperCore
    {
        public static IServiceCollection RegisterRepositoriesCore(this IServiceCollection services)
        {
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped(typeof(IRepository<,>), typeof(Repository<,>));
            services.AddScoped(typeof(IRepositoryBase<>), typeof(RepositoryBase<>));

            var repoClasses = Assembly.GetExecutingAssembly().GetTypes()
                .Where(x => x.IsClass
                            && (x.HasGenericInterface(typeof(IRepository<,>)) || x.HasGenericInterface(typeof(IRepositoryBase<>))
                                 || typeof(IUnitOfWork).IsAssignableFrom(x))
                            && x != typeof(Repository<>)
                            && x != typeof(Repository<,>)
                            && x != typeof(RepositoryBase<>)
                            && x != typeof(UserRepository<>)
                            && x != typeof(RoleRepository<>)
                            && x != typeof(UserUnitOfWork<,>)
                ).ToList();
            foreach (var type in repoClasses)
            {
                var interfaces = type.GetInterfaces();
                var @interface = interfaces.Last(x => (x.HasGenericInterface(typeof(IRepository<,>)) || x.HasGenericInterface(typeof(IRepositoryBase<>)) || typeof(IUnitOfWork).IsAssignableFrom(x))
                                                      && x.FullName != typeof(IUnitOfWork).FullName
                );
                services.AddScoped(@interface, type);
            }

            return services;
        }

        private static bool HasGenericInterface(this Type type, Type interfaceType)
        {
            return type.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == interfaceType);
        }

        public static IServiceCollection RegisterUserAndRole<TContextAdapter, TUserAdapter>(this IServiceCollection services)
            where TContextAdapter : class, IContextAdapter
            where TUserAdapter : class, IUserAdapter
        {
            var allTargetClasses = Assembly.GetEntryAssembly().GetTypes().Concat(Assembly.GetCallingAssembly().GetTypes());
            var targetClasses = allTargetClasses.Where(x => x.IsClass
                            && (x.HasGenericInterface(typeof(IUserRepository<>)) || x.HasGenericInterface(typeof(IRoleRepository<>)) || x.HasGenericInterface(typeof(IUserUnitOfWork<,>)))
                ).ToList();
            foreach (var type in targetClasses)
            {
                var interfaces = type.GetInterfaces();
                var @interface = interfaces.Last(x => (x.HasGenericInterface(typeof(IUserRepository<>)) || x.HasGenericInterface(typeof(IRoleRepository<>)) || typeof(IUnitOfWork).IsAssignableFrom(x))
                                                      && x.FullName != typeof(IUnitOfWork).FullName
                );

                services.AddScoped(@interface, type);
            }
            services.AddScoped<IContextAdapter, TContextAdapter>();
            services.AddScoped<IUserAdapter, TUserAdapter>();

            return services;
        }
    }

}
