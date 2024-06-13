using Microsoft.Extensions.Configuration;
using SiteWatcher.Application.Interfaces;

namespace SiteWatcher.Worker;

public class WorkerSettings
{
    [ConfigurationKeyName("Worker_EnableJobs")]
    public bool EnableJobs { get; set; }

    [ConfigurationKeyName("Worker_SiteWatcherUri")]
    public string SiteWatcherUri { get; set; } = null!;

    [ConfigurationKeyName("Worker_DbConnectionString")]
    public string DbConnectionString { get; set; } = null!;

    public string RedisConnectionString { get; set; } = null!;

    public string FrontEndUrl { get; set; } = null!;
}

public class WorkerAppSettings : IAppSettings
{
    public bool IsDevelopment { get; set; }
    public string ConnectionString { get; set; } = null!;
    public string FrontEndUrl { get; set; } = null!;
    public string FrontEndAuthUrl { get; set; } = null!;
    public byte[] RegisterKey { get; set; } = null!;
    public byte[] AuthKey { get; set; } = null!;
    public string RedisConnectionString { get; set; } = null!;
    public string CorsPolicy { get; set; } = null!;
    public byte[] InvalidToken { get; set; } = null!;
    public string ApiKeyName { get; set; } = null!;
    public string ApiKey { get; set; } = null!;
    public string IdHasherSalt { get; set; } = null!;
    public int MinimumHashedIdLength { get; set; }
    string IAppSettings.MessageIdKey => MessageIdKey;
    public const string MessageIdKey = "message-id";
    public bool InMemoryStorageAndQueue { get; set; }
    public bool DisableDataProtectionRedisStore { get; set; }
}

public sealed class HealthCheckSettings
{
    public const string TagDatabase = "db";
    public const string TagRabbitMq = "rabbitMq";
    public const string TagEmailHost = "email";

    [ConfigurationKeyName("HealtCheck_BaseHost")]
    public string BaseHost { get; set; } = null!;

    [ConfigurationKeyName("HealtCheck_BasePath")]
    public string BasePath { get; set; } = null!;

    [ConfigurationKeyName("HealtCheck_UiPath")]
    public string UiPath { get; set; } = null!;

    public string FullCheckPath => $"{BasePath}/full";
}