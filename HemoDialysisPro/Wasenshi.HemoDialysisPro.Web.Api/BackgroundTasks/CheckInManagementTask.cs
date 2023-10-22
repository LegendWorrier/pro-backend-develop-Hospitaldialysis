using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using ServiceStack.Messaging;
using System;
using System.Threading;
using System.Threading.Tasks;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Services;
using Wasenshi.HemoDialysisPro.Share.GlobalEvents;
using Wasenshi.HemoDialysisPro.Models.TimezoneUtil;
using Wasenshi.HemoDialysisPro.Web.Api.WebSocket;

namespace Wasenshi.HemoDialysisPro.Web.Api.BackgroundTasks
{
    public class CheckInManagementTask : BackgroundService
    {
        private readonly IServiceProvider services;
        private readonly IConfiguration config;
        private readonly IMessageService mq;

        private TimeZoneInfo tz;

        public CheckInManagementTask(IServiceProvider services, IConfiguration config, IMessageService mq)
        {
            this.services = services;
            this.config = config;
            this.mq = mq;

            tz = TimezoneUtils.GetTimeZone(config["TIMEZONE"]);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            mq.RegisterHandler<CheckInPostWeight>(m =>
            {
                // clear and discard staled one
                if (recordListener != null)
                {
                    ServiceEvents.OnDialysisRecordCreated -= recordListener;
                }
                if (hemosheetListener != null)
                {
                    ServiceEvents.OnHemosheetCreated -= hemosheetListener;
                }

                var hemoId = m.GetBody().HemoId;
                var connectionId = m.GetBody().ConnectionId;
                var patientId = m.GetBody().PatientId;

                if (!hemoId.HasValue)
                {
                    hemosheetListener = (HemodialysisRecord hemosheet) =>
                    {
                        if (hemosheet.PatientId == patientId)
                        {
                            hemoId = hemosheet.Id;
                            // signal to all hemo server instance to stop listening
                            using var scope = services.CreateScope();
                            var message = scope.ServiceProvider.GetRequiredService<IMessageQueueClient>();
                            message.Publish(new CheckInPostWeight
                            {
                                PatientId = patientId,
                                ConnectionId = connectionId,
                                HemoId = hemoId,
                            });
                        }
                    };
                    ServiceEvents.OnHemosheetCreated += hemosheetListener;
                }
                else
                {
                    recordListener = (DialysisRecord record) =>
                    {
                        if (record.HemodialysisId == hemoId && !record.IsFromMachine)
                        {
                            using var scope = services.CreateScope();
                            var hub = scope.ServiceProvider.GetRequiredService<IHubContext<CheckInHub>>();
                            hub.Clients.Client(connectionId).SendAsync("PostWeight", patientId).ConfigureAwait(true).GetAwaiter().GetResult();

                            // signal to all hemo server instance to stop listening
                            var message = scope.ServiceProvider.GetRequiredService<IMessageQueueClient>();
                            message.Publish(new CheckInPostWeightSignaled());
                        }
                    };
                    ServiceEvents.OnDialysisRecordCreated += recordListener;
                }

                return null;
            });

            // remove listener on this instance
            mq.RegisterHandler<CheckInPostWeightSignaled>(m =>
            {
                ServiceEvents.OnDialysisRecordCreated -= recordListener;

                return null;
            });
        }

        private ServiceEvents.DialysisRecordEvent recordListener;
        private ServiceEvents.HemosheetEvent hemosheetListener;

        public override void Dispose()
        {
            Log.Information("Disposing CheckIn management task...");
            base.Dispose();
        }
    }
}
