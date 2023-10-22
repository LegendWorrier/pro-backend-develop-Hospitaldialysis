using Microsoft.Extensions.DependencyInjection;

namespace Wasenshi.HemoDialysisPro.Repositories
{
    public static class RegisterHelper
    {
        public static IServiceCollection RegisterRepositories(this IServiceCollection services, bool hasIdentity = false)
        {
            services
                .RegisterRepositoriesCore()
                .AddTransient<IApplicationDbContext, ApplicationDbContext>() // should not be used directly (e.g. use pooling setup, via context adapter injection instead)
                .RegisterUserAndRole<ContextAdapterAspNet, UserAdapterAspNet>();

            if (hasIdentity)
            {
                services.AddTransient<UserLoginStore, UserLoginStore>();
            }

            return services;
        }
    }
}
