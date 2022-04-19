using SiteWatcher.Application.Interfaces;
using StackExchange.Redis;

namespace SiteWatcher.Data.Cache;

public class RedisCache : ICache
{
    private readonly IConnectionMultiplexer _connectionMultiplexer;

    public RedisCache(IConnectionMultiplexer connectionMultiplexer) =>
        this._connectionMultiplexer = connectionMultiplexer;

    public async Task SaveStringAsync(string key, string value, TimeSpan expiration) =>
        await _connectionMultiplexer.GetDatabase().StringSetAsync(key, value, expiration, flags: CommandFlags.FireAndForget);

    public async Task SaveBytesAsync(string key, byte[] value, TimeSpan expiration) =>
        await _connectionMultiplexer.GetDatabase().StringSetAsync(key, value, expiration, flags: CommandFlags.FireAndForget);

    public async Task<string> GetAndRemoveStringAsync(string key) =>
        await _connectionMultiplexer.GetDatabase().StringGetDeleteAsync(key);

    public async Task<byte[]?> GetAndRemoveBytesAsync(string key) =>
        await _connectionMultiplexer.GetDatabase().StringGetDeleteAsync(key);

    public async Task<string> GetStringAsync(string key) =>
        await _connectionMultiplexer.GetDatabase().StringGetAsync(key);

    public async Task<byte[]?> GetBytesAsync(string key) =>
        await _connectionMultiplexer.GetDatabase().StringGetAsync(key);
}