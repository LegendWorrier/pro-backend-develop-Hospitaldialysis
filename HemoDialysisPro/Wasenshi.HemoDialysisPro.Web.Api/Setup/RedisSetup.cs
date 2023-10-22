using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ServiceStack.Messaging;
using ServiceStack.Messaging.Redis;
using ServiceStack.Redis;
using StackExchange.Redis;
using Wasenshi.HemoDialysisPro.Web.Api.Utils;
using Wasenshi.HemoDialysisPro.Web.Api.WebSocket;

namespace Wasenshi.HemoDialysisPro.Web.Api.Setup
{
    public static class RedisSetup
    {
        public static IServiceCollection AddRedisAndMQ(this IServiceCollection services, IConfiguration configuration)
        {
            RedisConfig.DefaultMaxPoolSize = configuration.GetValue<int?>("RedisPoolSize") ?? 1000;
            var redisManager = new PooledRedisClientManager(configuration["RedisConnection"]);
            redisManager.IdleTimeOutSecs = 30;
            redisManager.PoolTimeout = 3;
            redisManager.ConnectTimeout = 5;

            var redisServer = new RedisMqServer(redisManager, retryCount: 2);
            redisServer.DisablePriorityQueues = true;

            services
                .AddSingleton<IRedisClientsManager>(redisManager)
                .AddSingleton<IMessageService>(redisServer)
                .AddSingleton<IMessageFactory>(c => c.GetRequiredService<IMessageService>().MessageFactory)
                .AddScoped<IRedisClient>(c => c.GetRequiredService<IRedisClientsManager>().GetClient())
                .AddScoped<IMessageProducer>(c => c.GetRequiredService<IMessageFactory>().CreateMessageProducer())
                .AddScoped<IMessageQueueClient>(c => c.GetRequiredService<IMessageFactory>().CreateMessageQueueClient());

            // extra redis pool for stack exchange base (used by notification system which depends on RediSearch)
            var redisPool = new RedisPool(10, ConfigurationOptions.Parse(configuration["RedisConnection"]));
            services.AddSingleton<IRedisPool>(redisPool);

            // setup index
            redisPool.CreateNotificationIndex();

            return services;
        }
    }
}
