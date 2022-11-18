using SiteWatcher.Application.Interfaces;

namespace SiteWatcher.Worker;

public class WorkerSettings
{
    public bool EnableJobs { get; set; }
    public bool UseInMemoryStorageAndQueue { get; set; }
    public RabbitMqSettings RabbitMq { get; set; } = null!;
    public WorkerAppSettings AppSettings { get; set; } = null!;
    public string SiteWatcherUri { get; set; } = null!;
    public EmailSettings EmailSettings { get; set; } = null!;
}

public class RabbitMqSettings
{
    public string Host { get; set; } = null!;
    public string UserName { get; set; } = null!;
    public string Password { get; set; } = null!;
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
    public string FromEmail { get; set; } = null!;
    public string FromName { get; set; } = null!;
    public string ReplyToEmail { get; set; } = null!;
    public string ReplyToName { get; set; } = null!;
    public string SmtpHost { get; set; } = null!;
    public string SmtpUser { get; set; } = null!;
    public string SmtpPassword { get; set; } = null!;
    public int Port { get; set; }
    public bool UseTls {get; set; }
    public int EmailDelaySeconds { get; set; }
}