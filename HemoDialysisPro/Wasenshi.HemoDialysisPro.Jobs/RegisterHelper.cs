using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ServiceStack;
using ServiceStack.Messaging;
using ServiceStack.Messaging.Redis;
using ServiceStack.Redis;
using Wasenshi.HemoDialysisPro.Models.Settings;
using Wasenshi.HemoDialysisPro.Repositories;
using Wasenshi.HemoDialysisPro.Repository.Core.Interfaces.Base;
using Wasenshi.HemoDialysisPro.Services;
using Wasenshi.HemoDialysisPro.Share;
using Wasenshi.HemoDialysisPro.Models.ConfigExtesions;

namespace Wasenshi.HemoDialysisPro.Jobs
{
    public static class RegisterHelper
    {
        /// <summary>
        /// Use this in Job Server. (If used in Web API, the repository and dependencies will be double-registered)
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection RegisterJobsAndDependencies<TContextAdapter, TUserAdapter>(this IServiceCollection services, IConfiguration config)
            where TContextAdapter : class, IContextAdapter
            where TUserAdapter : class, IUserAdapter
        {
            services
                .AddSingleton(config)
                .RegisterRepositoriesCore()
                .RegisterServicesCore()
                .RegisterUserAndRole<TContextAdapter, TUserAdapter>();

            var unitSetting = config.GetSection("UnitSettings");
            services.ConfigureWritable<UnitSettings>(unitSetting);
            var globalSetting = config.GetSection("GlobalSettings");
            services.ConfigureWritable<GlobalSetting>(globalSetting);

            // register jobs
            services
                .AddTransient<RefreshTokenJob>()
                .AddTransient<ScheduleManageJob>()
                .AddTransient<ShiftManagementJob>()
                .AddTransient<HemosheetJob>()
                .AddTransient<HemoBoxJob>();


            return services;
        }

        public static IServiceCollection RegisterRedis(this IServiceCollection services, IConfiguration config)
        {
            var redisManager = new PooledRedisClientManager(config["RedisConnection"]);
            redisManager.IdleTimeOutSecs = 30;
            redisManager.PoolTimeout = 3;
            redisManager.ConnectTimeout = 5;

            var redisServer = new RedisMqServer(redisManager, retryCount: 2);
            redisServer.DisablePriorityQueues = true;

            redisServer.RegisterHandler<SectionUpdated>(ShiftManagementJob.OnSectionUpdated);
            redisServer.RegisterHandler<StartNextRound>(ShiftManagementJob.OnStartNextRound);

            services
                .AddSingleton<IRedisClientsManager>(redisManager)
                .AddSingleton<IMessageService>(redisServer)
                .AddSingleton<IMessageFactory>(c => c.GetRequiredService<IMessageService>().MessageFactory)
                .AddScoped<IRedisClient>(c => c.GetRequiredService<IRedisClientsManager>().GetClient())
                .AddScoped<IMessageProducer>(c => c.GetRequiredService<IMessageFactory>().CreateMessageProducer())
                .AddScoped<IMessageQueueClient>(c => c.GetRequiredService<IMessageFactory>().CreateMessageQueueClient());

            redisServer.Start();

            return services;
        }
    }
}
