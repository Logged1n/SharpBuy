using Application.Abstractions.Caching;
using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;

namespace Infrastructure.Caching;

internal sealed class CacheInvalidator(IConnectionMultiplexer redis) : ICacheInvalidator
{
    public async Task InvalidateByPatternAsync(string pattern, CancellationToken cancellationToken = default)
    {
        IServer server = redis.GetServer(redis.GetEndPoints()[0]);

        // Add instance name prefix if configured (SharpBuy:)
        string searchPattern = $"SharpBuy:{pattern}";

        await foreach (RedisKey key in server.KeysAsync(pattern: searchPattern))
        {
            await redis.GetDatabase().KeyDeleteAsync(key);
        }
    }

    public async Task InvalidateAsync(string key, CancellationToken cancellationToken = default)
    {
        // Add instance name prefix if configured (SharpBuy:)
        string redisKey = $"SharpBuy:{key}";
        await redis.GetDatabase().KeyDeleteAsync(redisKey);
    }

    public async Task InvalidateMultipleAsync(IEnumerable<string> keys, CancellationToken cancellationToken = default)
    {
        IDatabase database = redis.GetDatabase();
        RedisKey[] redisKeys = keys.Select(k => (RedisKey)$"SharpBuy:{k}").ToArray();
        await database.KeyDeleteAsync(redisKeys);
    }
}
