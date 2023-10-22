using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Wasenshi.HemoDialysisPro.Models
{
    public static class DistributedCacheHelper
    {
        public static async Task<T> GetOrSetDistributedCacheAsync<T>(this IDistributedCache distributedCache, string key, Func<Task<T>> valueSetter, DistributedCacheEntryOptions options = null) where T : class
        {
            var jsonOption = new JsonSerializerOptions
            {
                MaxDepth = 10
            };

            var cached = await distributedCache.GetAsync(key);
            if (cached == null)
            {
                T value = await valueSetter();

                if (value != null)
                {
                    var json = JsonSerializer.Serialize(value, jsonOption);
                    var bytes = Encoding.UTF8.GetBytes(json);
                    options ??= new DistributedCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromHours(15));
                    distributedCache.SetAsync(key, bytes, options);
                }

                return value;
            }

            var serialized = Encoding.UTF8.GetString(cached);
            var result = JsonSerializer.Deserialize<T>(serialized, jsonOption);
            return result;
        }
    }
}
