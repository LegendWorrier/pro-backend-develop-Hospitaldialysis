using StackExchange.Redis;
using System.Threading.Tasks;

namespace Wasenshi.HemoDialysisPro.Web.Api.Utils
{
    public interface IRedisPool
    {
        Task<IDatabase> GetDatabaseAsync();
    }

    public class RedisPool : IRedisPool
    {
        private readonly ConnectionMultiplexer[] _pool;
        private readonly ConfigurationOptions _redisConfigurationOptions;

        public RedisPool(int poolSize, string connectionString) : this(poolSize, ConfigurationOptions.Parse(connectionString))
        {
        }

        public RedisPool(int poolSize, ConfigurationOptions redisConfigurationOptions)
        {
            _pool = new ConnectionMultiplexer[poolSize];
            _redisConfigurationOptions = redisConfigurationOptions;
        }

        public async Task<IDatabase> GetDatabaseAsync()
        {
            var leastPendingTasks = long.MaxValue;
            IDatabase leastPendingDatabase = null;

            for (int i = 0; i < _pool.Length; i++)
            {
                var connection = _pool[i];

                if (connection == null)
                {
                    _pool[i] = await ConnectionMultiplexer.ConnectAsync(_redisConfigurationOptions);

                    return _pool[i].GetDatabase();
                }

                var pending = connection.GetCounters().TotalOutstanding;

                if (pending < leastPendingTasks)
                {
                    leastPendingTasks = pending;
                    leastPendingDatabase = connection.GetDatabase();
                }
            }

            return leastPendingDatabase;
        }
    }
}
