using Microsoft.Extensions.Configuration;

namespace SiteWatcher.Infra.EmailSending;

public class EmailSettings
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