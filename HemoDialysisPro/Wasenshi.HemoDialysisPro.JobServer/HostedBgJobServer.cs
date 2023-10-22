using Hangfire;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wasenshi.HemoDialysisPro.JobServer
{
    public class HostedBgJobServer : BackgroundService
    {
        public static BackgroundJobServer? Instance { get; private set; }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Instance = new BackgroundJobServer(new BackgroundJobServerOptions()
            {
                WorkerCount = 1
            });

            return Task.CompletedTask;
        }

        public override void Dispose()
        {
            base.Dispose();
            Instance?.Dispose();
        }
    }
}
