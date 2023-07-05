using Microsoft.Extensions.Configuration;

namespace SiteWatcher.Infra.Messaging;

public class RabbitMqSettings
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
    public ushort Port { get; set; }
    public const string SiteWatcherExchange = "site-watcher";
}