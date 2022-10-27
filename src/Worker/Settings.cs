using SiteWatcher.Application.Interfaces;
using SiteWatcher.Domain.Enums;

namespace SiteWatcher.Worker;

public class WorkerSettings
{
    public WorkerSettings()
    {
        Triggers = new Dictionary<EFrequency, string>();
    }

    public bool EnableJobs { get; set; }

    public Dictionary<EFrequency, string> Triggers { get; set; }

    public bool UseInMemoryStorageAndQueue { get; set; }
    public RabbitMqSettings RabbitMq { get; set; } = null!;
    public WorkerAppSettings AppSettings { get; set; } = null!;
    public ConsumerSettings Consumers { get; set; } = null!;
}

public class RabbitMqSettings
{
    public string Host { get; set; } = null!;
    public string UserName { get; set; } = null!;
    public string Password { get; set; } = null!;
    public int Port { get; set; }
}

public class ConsumerSettings
{
    // Set zero to use the processor count
    public int PerQueueConcurrency { get; set; }
}

public static class Exchanges
{
    public const string SiteWatcher = "site-watcher";
}

public static class RoutingKeys
{
    public const string WatchAlerts = "site-watcher.worker.watch-alerts";
}

public static class MessageHeaders
{
    public const string MessageIdKey = "message-id";
}

public class WorkerAppSettings : IAppSettings
{
    public bool IsDevelopment { get; set; }
    public string ConnectionString { get; set; } = null!;
    public string FrontEndUrl { get; set; } = null!;
    public byte[] RegisterKey { get; set; } = null!;
    public byte[] AuthKey { get; set; } = null!;
    public string RedisConnectionString { get; set; } = null!;
    public string CorsPolicy { get; set; } = null!;
    public byte[] InvalidToken { get; set; } = null!;
    public string ApiKeyName { get; set; } = null!;
    public string ApiKey { get; set; } = null!;
    public string IdHasherSalt { get; set; } = null!;
    public int MinimumHashedIdLength { get; set; }
}