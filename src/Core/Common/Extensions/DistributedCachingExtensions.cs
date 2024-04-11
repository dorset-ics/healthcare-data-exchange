using System.Text;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using Task = System.Threading.Tasks.Task;

namespace Core.Common.Extensions;

public static class DistributedCachingExtensions
{

    public static Task SetAsync<T>(this IDistributedCache distributedCache, string key, T value, DistributedCacheEntryOptions options, CancellationToken token = default)
    {
        return distributedCache.SetAsync(key, Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(value)), options, token);
    }

    public async static Task<T?> GetAsync<T>(this IDistributedCache distributedCache, string key, CancellationToken token = default)
    {
        var result = await distributedCache.GetAsync(key, token);
        return result == null ? default : JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(result));
    }

}