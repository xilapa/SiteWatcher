using System.Text.Json;
using SiteWatcher.Application.Interfaces;

namespace SiteWatcher.IntegrationTests.Setup.TestServices;

public class FakeCache : ICache
{
    public Dictionary<string, object> Cache { get; } = new ();

    public Task SaveStringAsync(string key, string value, TimeSpan expiration)
    {
        Cache.TryAdd(key, value);
        return Task.CompletedTask;
    }

    public Task SaveBytesAsync(string key, byte[] value, TimeSpan expiration)
    {
        Cache.TryAdd(key, value);
        return Task.CompletedTask;
    }

    public Task<string?> GetAndRemoveStringAsync(string key)
    {
        var result = Cache.TryGetValue(key, out var value) ? value.ToString() : null;
        Cache.Remove(key);
        return Task.FromResult(result);
    }

    public Task<byte[]?> GetAndRemoveBytesAsync(string key)
    {
        Cache.TryGetValue(key, out var value);
        Cache.Remove(key);
        return Task.FromResult(value as byte[] ?? null);
    }

    public Task<string?> GetStringAsync(string key)
    {
        var result = Cache.TryGetValue(key, out var value) ? value.ToString() : null;
        return Task.FromResult(result);
    }

    public Task<byte[]?> GetBytesAsync(string key)
    {
        Cache.TryGetValue(key, out var value);
        return Task.FromResult(value as byte[] ?? null);
    }

    public Task SaveAsync(string key, object? value, TimeSpan expiration)
    {
        if(value is null) return Task.CompletedTask;
        var objectJson = JsonSerializer.Serialize(value);
        Cache.TryAdd(key, objectJson);
        return Task.CompletedTask;
    }

    public Task<T?> GetAsync<T>(string key)
    {
        var objectJson = Cache.TryGetValue(key, out var value) ? value.ToString() : null;
        if (string.IsNullOrEmpty(objectJson)) return Task.FromResult<T?>(default);
        var @object = JsonSerializer.Deserialize<T>(objectJson);
        return Task.FromResult(@object);
    }
}