using System.Text.Json;
using SiteWatcher.Application.Interfaces;
using StackExchange.Redis;

namespace SiteWatcher.Data.Cache;

public class RedisCache : ICache
{
    private readonly IConnectionMultiplexer _connectionMultiplexer;

    public RedisCache(IConnectionMultiplexer connectionMultiplexer) =>
        _connectionMultiplexer = connectionMultiplexer;

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

    public async Task SaveAsync(string key, object? value, TimeSpan expiration)
    {
        if(value is null) return;
        var objectJson = JsonSerializer.Serialize(value);
        await _connectionMultiplexer.GetDatabase().StringSetAsync(key, objectJson, expiration, flags: CommandFlags.FireAndForget);
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        var objectJson = await _connectionMultiplexer.GetDatabase().StringGetAsync(key);
        if (objectJson.IsNullOrEmpty) return default;
        var @object = JsonSerializer.Deserialize<T>(objectJson);
        return @object;
    }
}