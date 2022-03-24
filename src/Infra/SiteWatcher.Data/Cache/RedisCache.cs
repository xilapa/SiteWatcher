using SiteWatcher.Domain.Interfaces;
using StackExchange.Redis;

namespace SiteWatcher.Infra.Cache;

public class RedisCache : ICache
{
    private readonly IConnectionMultiplexer connectionMultiplexer;

    public RedisCache(IConnectionMultiplexer connectionMultiplexer) =>    
        this.connectionMultiplexer = connectionMultiplexer;

    public async Task SaveStringAsync(string key, string value, TimeSpan expiration) =>
        await connectionMultiplexer.GetDatabase().StringSetAsync(key, value, expiration, flags: CommandFlags.FireAndForget);

    public async Task SaveBytesAsync(string key, byte[] value, TimeSpan expiration) =>
        await connectionMultiplexer.GetDatabase().StringSetAsync(key, value, expiration, flags: CommandFlags.FireAndForget);

    public async Task<string> GetAndRemoveStringAsync(string key) =>
        await connectionMultiplexer.GetDatabase().StringGetDeleteAsync(key);

    public async Task<byte[]> GetAndRemoveBytesAsync(string key) =>
        await connectionMultiplexer.GetDatabase().StringGetDeleteAsync(key);

    public async Task<string> GetStringAsync(string key) =>
        await connectionMultiplexer.GetDatabase().StringGetAsync(key);

    public async Task<byte[]> GetBytesAsync(string key) =>
        await connectionMultiplexer.GetDatabase().StringGetAsync(key);

}