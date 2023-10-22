using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using Weikio.PluginFramework.Abstractions;

namespace Wasenshi.HemoDialysisPro.PluginIntegrate
{
    public class PluginTaskScheduler : IHostedService
    {
        private readonly IServiceProvider services;
        private readonly ILogger<PluginTaskScheduler> _logger;

        private Timer _workTimer;
        private static PluginTaskScheduler instance_;

        public PluginTaskScheduler(IServiceProvider services, ILogger<PluginTaskScheduler> logger)
        {
            this.services = services;
            _logger = logger;

            instance_ = this;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Initializing plugin background task scheduler");

                _workTimer = new Timer(async (o) => await DoWork(o), null, TimeSpan.Zero, TimeSpan.FromSeconds(5));
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to initialize plugin catalogs");

                throw;
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public async Task DoWork(object state)
        {
            while (Queue.TryDequeue(out var work))
            {
                using var scope = services.CreateScope();
                try
                {
                    await work(scope);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"[PLUGIN] Failed to execute plugin task queue.");
                }
                finally
                {
                    scope.Dispose();
                }
            }
        }

        internal static readonly ConcurrentQueue<Func<IServiceScope, Task>> Queue = new ConcurrentQueue<Func<IServiceScope, Task>>();

        public static void AddWorkToQueue(Func<IServiceScope, Task> work)
        {
            Queue.Enqueue(work);
        }

        public static void StartImmediately()
        {
            TrickerQueueTimer();
        }

        public static void TrickerQueueTimer()
        {
            instance_?._workTimer.Change(Timeout.Infinite, Timeout.Infinite); // stop
            instance_?._workTimer.Change(TimeSpan.Zero, TimeSpan.FromSeconds(5)); // start again
        }
    }
}
