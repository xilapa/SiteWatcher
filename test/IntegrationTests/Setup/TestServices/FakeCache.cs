using System.Text.Json;
using SiteWatcher.Application.Interfaces;

namespace SiteWatcher.IntegrationTests.Setup.TestServices;

public class FakeCache : ICache
{
    public Dictionary<string, FakeCacheEntry> Cache { get; } = new ();

    public Task SaveStringAsync(string key, string value, TimeSpan expiration)
    {
        Cache.TryAdd(key, new FakeCacheEntry(value, expiration));
        return Task.CompletedTask;
    }

    public Task SaveBytesAsync(string key, byte[] value, TimeSpan expiration)
    {
        Cache.TryAdd(key, new FakeCacheEntry(value, expiration));
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
        Cache.TryGetValue(key, out var entry);
        Cache.Remove(key);
        return Task.FromResult(entry.Value as byte[] ?? null);
    }

    public Task<string?> GetStringAsync(string key)
    {
        var result = Cache.TryGetValue(key, out var value) ? value.ToString() : null;
        return Task.FromResult(result);
    }

    public Task<byte[]?> GetBytesAsync(string key)
    {
        Cache.TryGetValue(key, out var entry);
        return Task.FromResult(entry.Value as byte[] ?? null);
    }

    public Task SaveAsync(string key, object? value, TimeSpan expiration)
    {
        if(value is null) return Task.CompletedTask;
        var objectJson = JsonSerializer.Serialize(value);
        Cache.TryAdd(key, new FakeCacheEntry(objectJson, expiration));
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

public struct FakeCacheEntry
{
    public FakeCacheEntry(object value, TimeSpan expiration)
    {
        Value = value;
        Expiration = expiration;
    }

    public object Value { get; }
    public TimeSpan Expiration { get; }
}