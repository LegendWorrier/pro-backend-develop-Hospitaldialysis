using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Wasenshi.HemoDialysisPro.Share
{
    public sealed class LicenseCheckTask : IHostedService, IDisposable
    {
        private int executionCount = 0;
        private Timer _timer;
        private readonly ILogger<LicenseCheckTask> logger;

        public LicenseCheckTask(ILogger<LicenseCheckTask> logger)
        {
            this.logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("License Check Task running. [{AppDomain}]", AppDomain.CurrentDomain.FriendlyName);

            _timer = new Timer(DoWork, null, TimeSpan.Zero,
                TimeSpan.FromHours(6));

            return Task.CompletedTask;
        }

        private void DoWork(object state)
        {
            var count = Interlocked.Increment(ref executionCount);

            logger.LogInformation(
                "Checking license.... Count: {Count}", count);
            LicenseManager.CheckLicense();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("License Check Task is stopping.");

            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
