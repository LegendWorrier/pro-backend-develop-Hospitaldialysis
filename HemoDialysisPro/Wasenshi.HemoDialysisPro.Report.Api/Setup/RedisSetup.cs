using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ServiceStack.Messaging;
using ServiceStack.Messaging.Redis;
using ServiceStack.Redis;
using StackExchange.Redis;

namespace Wasenshi.HemoDialysisPro.Report.Api.Setup
{
    public static class RedisSetup
    {
        public static IServiceCollection AddRedis(this IServiceCollection services, IConfiguration configuration)
        {
            RedisConfig.DefaultMaxPoolSize = configuration.GetValue<int?>("RedisPoolSize") ?? 1000;
            var redisManager = new PooledRedisClientManager(configuration["RedisConnection"]);
            redisManager.IdleTimeOutSecs = 30;
            redisManager.PoolTimeout = 3;
            redisManager.ConnectTimeout = 5;

            services
                .AddSingleton<IRedisClientsManager>(redisManager)
                .AddScoped<IRedisClient>(c => c.GetRequiredService<IRedisClientsManager>().GetClient());

            return services;
        }
    }
}
