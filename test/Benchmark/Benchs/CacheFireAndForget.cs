using System.Text.Json;
using BenchmarkDotNet.Attributes;
using StackExchange.Redis;

namespace SiteWatcher.Benchmark.Benchs;

[MemoryDiagnoser]
[MinColumn, MaxColumn]
public class CacheFireAndForget
{
    private IConnectionMultiplexer _connectionMultiplexer = null!;
    private IDatabase _redisCache = null!;
    private readonly string _value;

    public CacheFireAndForget()
    {
        var person = new Person
        {
            Name = "Name",
            BirthDay = DateTime.Now,
            Address = new Address {City = "City", Street = "Street", State = "State", Zip = "Zip"}
        };

        _value = JsonSerializer.Serialize(person);
    }

    [GlobalSetup]
    public void CacheSetup()
    {
        var configOptions = ConfigurationOptions.Parse("localhost:6379");
        configOptions.AbortOnConnectFail = true;

        _connectionMultiplexer = ConnectionMultiplexer.Connect(configOptions);
        _redisCache = _connectionMultiplexer.GetDatabase();
    }

    [GlobalCleanup]
    public async Task CacheDispose()
    {
        await Task.Delay(5000);
        await _connectionMultiplexer.CloseAsync();
        _connectionMultiplexer.Dispose();
    }

    [Benchmark]
    public Task FireAndForget()
    {
        return _redisCache.StringSetAsync("fireandforget", _value, TimeSpan.FromMinutes(5), flags: CommandFlags.FireAndForget);
    }

    [Benchmark]
    public async Task WaitCacheResponse()
    {
        await _redisCache.StringSetAsync("awaitresponse", _value, TimeSpan.FromMinutes(5));
    }
}

public class Person
{
    public string Name { get; set; }
    public DateTime BirthDay { get; set; }
    public Address Address { get; set; }
}

public record Address
{
    public string Street { get; set; }
    public string City { get; set; }
    public string State { get; set; }
    public string Zip { get; set; }
}