using Microsoft.Extensions.Configuration;
using SiteWatcher.Application.Interfaces;

namespace SiteWatcher.Worker;

public class WorkerSettings
{
    [ConfigurationKeyName("Worker_EnableJobs")]
    public bool EnableJobs { get; set; }

    [ConfigurationKeyName("Worker_UseInMemoryStorageAndQueue")]
    public bool UseInMemoryStorageAndQueue { get; set; }

    [ConfigurationKeyName("Worker_SiteWatcherUri")]
    public string SiteWatcherUri { get; set; } = null!;

    [ConfigurationKeyName("Worker_DbConnectionString")]
    public string DbConnectionString { get; set; } = null!;
}

public class RabbitMqSettings : IRabbitMqSettings
{
    [ConfigurationKeyName("RabbitMq_Host")]
    public string Host { get; set; } = null!;

    [ConfigurationKeyName("RabbitMq_VirtualHost")]
    public string VirtualHost { get; set; } = null!;

    [ConfigurationKeyName("RabbitMq_UserName")]
    public string UserName { get; set; } = null!;

    [ConfigurationKeyName("RabbitMq_Password")]
    public string Password { get; set; } = null!;

    [ConfigurationKeyName("RabbitMq_Port")]
    public int Port { get; set; }
}

public static class Exchanges
{
    public const string SiteWatcher = "site-watcher";
}

public static class RoutingKeys
{
    public const string WatchAlerts = "site-watcher.worker.watch-alerts";
    public const string EmailNotification = "site-watcher.worker.notifications.email";
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

public class EmailSettings : IEmailSettings
{
    [ConfigurationKeyName("EmailSettings_FromEmail")]
    public string FromEmail { get; set; } = null!;

    [ConfigurationKeyName("EmailSettings_FromName")]
    public string FromName { get; set; } = null!;

    [ConfigurationKeyName("EmailSettings_ReplyToEmail")]
    public string ReplyToEmail { get; set; } = null!;

    [ConfigurationKeyName("EmailSettings_ReplyToName")]
    public string ReplyToName { get; set; } = null!;

    [ConfigurationKeyName("EmailSettings_SmtpHost")]
    public string SmtpHost { get; set; } = null!;

    [ConfigurationKeyName("EmailSettings_SmtpUser")]
    public string SmtpUser { get; set; } = null!;

    [ConfigurationKeyName("EmailSettings_SmtpPassword")]
    public string SmtpPassword { get; set; } = null!;

    [ConfigurationKeyName("EmailSettings_Port")]
    public int Port { get; set; }

    [ConfigurationKeyName("EmailSettings_UseTls")]
    public bool UseSsl { get; set; }

    [ConfigurationKeyName("EmailSettings_EmailDelaySeconds")]
    public int EmailDelaySeconds { get; set; }
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