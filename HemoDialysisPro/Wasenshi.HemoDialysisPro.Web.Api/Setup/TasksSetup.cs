using Microsoft.Extensions.DependencyInjection;
using Wasenshi.HemoDialysisPro.Web.Api.BackgroundTasks;

namespace Wasenshi.HemoDialysisPro.Web.Api.Setup
{
    public static class TasksSetup
    {
        public static void AddBackgroundTasks(this IServiceCollection services)
        {
            services.AddHostedService<RefreshTokenManagementTask>();
            services.AddHostedService<ShiftManagementTask>();
            services.AddHostedService<ScheduleManagementTask>();
            services.AddHostedService<HemosheetManagementTask>();
            services.AddHostedService<HemoBoxManagementTask>();
            services.AddHostedService<CheckInManagementTask>();
            services.AddHostedService<NotificationTask>();
        }
    }
}
