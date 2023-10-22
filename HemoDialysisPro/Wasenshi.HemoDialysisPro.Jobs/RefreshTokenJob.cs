using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using Wasenshi.HemoDialysisPro.Repository.Core.Interfaces.Base;

namespace Wasenshi.HemoDialysisPro.Jobs
{
    public class RefreshTokenJob
    {
        private readonly IConfiguration config;
        private readonly IServiceProvider services;
        private readonly ILogger<RefreshTokenJob> logger;

        public RefreshTokenJob(
            IConfiguration config,
            IServiceProvider services,
            ILogger<RefreshTokenJob> logger)
        {
            this.config = config;
            this.services = services;
            this.logger = logger;
        }

        public void PrunRefreshToken()
        {
            logger.LogInformation("Refresh Token Clear triggered.");
            using (var scope = services.CreateScope())
            {
                var user = scope.ServiceProvider.GetRequiredService<IUserAdapter>();
                user.ClearRefreshToken();
            }
        }
    }
}
