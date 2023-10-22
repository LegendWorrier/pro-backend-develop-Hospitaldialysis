using Hangfire;
using Microsoft.Extensions.DependencyInjection;

namespace Wasenshi.HemoDialysisPro.JobsServer
{
    public class ServiceProviderAdapter : JobActivator
    {
        private readonly IServiceProvider provider;

        public ServiceProviderAdapter(IServiceProvider provider)
        {
            this.provider = provider;
        }

        public override object ActivateJob(Type jobType)
        {
            return provider.GetRequiredService(jobType);
        }
    }
}
