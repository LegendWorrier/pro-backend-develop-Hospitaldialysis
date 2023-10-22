using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ServiceStack.Messaging;
using ServiceStack.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Wasenshi.HemoDialysisPro.Share;
using Wasenshi.HemoDialysisPro.Models.TimezoneUtil;
using Wasenshi.HemoDialysisPro.Web.Api.WebSocket;

namespace Wasenshi.HemoDialysisPro.Web.Api.BackgroundTasks
{
    public class NotificationTask : BackgroundService
    {
        private TimeZoneInfo tz;
        private readonly IConfiguration config;
        private readonly IMessageService mq;
        private readonly IServiceProvider services;

        public NotificationTask(IConfiguration config, IMessageService mq, IServiceProvider services)
        {
            this.config = config;
            this.mq = mq;
            this.services = services;
            tz = TimezoneUtils.GetTimeZone(config.GetValue<string>("TIMEZONE"));
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            mq.RegisterHandler<NotificationEvent>(m =>
            {
                var target = m.GetBody().Target;

                using var scope = services.CreateScope();
                var userHub = scope.ServiceProvider.GetRequiredService<IHubContext<UserHub, IUserClient>>();
                var redis = scope.ServiceProvider.GetRequiredService<IRedisClient>();

                var notification = redis.GetNotification(m.GetBody().NotificationId);
                var localizer = scope.ServiceProvider.GetRequiredService<IHtmlLocalizer<Notification>>();
                var notiInfo = notification.GetNotiInfo(localizer, config.GetValue<string>("CULTURE"));

                INotifiable targetClients;

                if (target.Roles?.Any() ?? false)
                {
                    List<string> groups;
                    if (target.Units?.Any() ?? false)
                    {
                        groups = target.Roles.SelectMany(r => target.Units.Select(u => UserHub.GetUnitRoleChannel(u, r)))
                            .Concat((target.Users ?? Enumerable.Empty<Guid>()).Select(x => x.ToString()))
                            .Concat(new[] { UserHub.ROOT_ADMIN }).ToList();
                    }
                    else
                    {
                        groups = target.Roles.Select(x => UserHub.GetRoleChannel(x))
                            .Concat((target.Users ?? Enumerable.Empty<Guid>()).Select(x => x.ToString()))
                            .ToList();
                    }
                    targetClients = userHub.Clients.Groups(groups);
                }
                else if (target.Units?.Any() ?? false)
                {
                    targetClients = userHub.Clients.Groups(target.Units.Select(x => UserHub.GetUnitChannelName(x)).Concat(new[] { UserHub.ROOT_ADMIN }).ToList());
                }
                else if (target.Users?.Any() ?? false)
                {
                    targetClients = userHub.Clients.Users(target.Users.Select(x => x.ToString()).ToList());
                }
                else
                {
                    targetClients = userHub.Clients.All;
                }

                targetClients.Notify(notiInfo);

                return null;
            }, (IMessageHandler handler, IMessage<NotificationEvent> message, Exception e) =>
            {
                using var scope = services.CreateScope();
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<NotificationTask>>();
                logger.LogError(e, "Error while process notification");
            });

            return Task.CompletedTask;
        }
    }
}
