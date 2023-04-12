using System.Text.Json;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Domain.Common.Services;

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
        Cache.TryGetValue(key, out var cacheEntry);
        if (cacheEntry.Value is null)
            return Task.FromResult<string?>(default);
        Cache.Remove(key);
        return Task.FromResult(cacheEntry.Value.ToString());
    }

    public Task<byte[]?> GetAndRemoveBytesAsync(string key)
    {
        Cache.TryGetValue(key, out var entry);
        Cache.Remove(key);
        return Task.FromResult(entry.Value as byte[] ?? null);
    }

    public Task<string?> GetStringAsync(string key)
    {
        Cache.TryGetValue(key, out var cacheEntry);
        return cacheEntry.Value is null ? Task.FromResult<string?>(default)
            : Task.FromResult(cacheEntry.Value.ToString());
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
        var objectJson = Cache.TryGetValue(key, out var fakeCacheEntry) ?
            fakeCacheEntry.Value.ToString() : null;
        if (string.IsNullOrEmpty(objectJson)) return Task.FromResult<T?>(default);
        var @object = JsonSerializer.Deserialize<T>(objectJson);
        return Task.FromResult(@object);
    }

    public Task<T?> GetAndRemoveAsync<T>(string key, CancellationToken ct)
    {
        var objectJson = Cache.TryGetValue(key, out var fakeCacheEntry) ?
            fakeCacheEntry.Value.ToString() : null;
        if (string.IsNullOrEmpty(objectJson)) return Task.FromResult<T?>(default);
        var @object = JsonSerializer.Deserialize<T>(objectJson);
        Cache.Remove(key);
        return Task.FromResult(@object);
    }

    public Task SaveHashAsync(string key, string fieldName, object fieldValue, TimeSpan expiration)
    {
        var objectJson = JsonSerializer.Serialize(fieldValue);
        var fakeEntry = new FakeCacheEntry(objectJson, expiration);
        var joinedkey = $"{key}:{fieldName}";
        Cache.TryAdd(joinedkey, fakeEntry);
        return  Task.CompletedTask;
    }

    public Task<string?> GetHashFieldAsStringAsync(string key, string fieldName)
    {
        var joinedkey = $"{key}:{fieldName}";
        Cache.TryGetValue(joinedkey, out var result);
        return Task.FromResult(result.Value as string);
    }

    public Task DeleteKeyAsync(string key)
    {
        var keyToRemove = Cache.Keys.FirstOrDefault(k => k.StartsWith(key));
        if(keyToRemove is null)
            return Task.CompletedTask;
        Cache.Remove(keyToRemove);
        return Task.CompletedTask;
    }

    public Task DeleteKeysWith(string partialKey)
    {
        throw new NotImplementedException();
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