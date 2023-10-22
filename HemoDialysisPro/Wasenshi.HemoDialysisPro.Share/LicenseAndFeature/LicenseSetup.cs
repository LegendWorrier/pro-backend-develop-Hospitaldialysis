using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Wasenshi.HemoDialysisPro.Share
{
    public static class LicenseSetup
    {
        public static IServiceCollection AddLicenseProtect(this IServiceCollection services)
        {
            return services.AddHostedService<LicenseCheckTask>();
        }

        public static IServiceCollection AddLicenseProtectWithLog(this IServiceCollection services)
        {
            var provider = services.BuildServiceProvider();
            var logFactory = provider.GetRequiredService<ILoggerFactory>();
            var logger = logFactory.CreateLogger(nameof(LicenseManager));
            LicenseManager.SetLogger(logger);
            logger = logFactory.CreateLogger(nameof(FeatureFlag));
            FeatureFlag.SetLogger(logger);

            return services.AddLicenseProtect();
        }
    }
}
