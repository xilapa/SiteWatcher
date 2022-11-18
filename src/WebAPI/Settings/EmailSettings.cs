using SiteWatcher.Application.Interfaces;

namespace SiteWatcher.WebAPI.Settings;

public class EmailSettings : IEmailSettings
{
    public string FromEmail { get; set; } = null!;
    public string FromName { get; set; } = null!;
    public string ReplyToEmail { get; set; } = null!;
    public string ReplyToName { get; set; } = null!;
    public string SmtpHost { get; set; } = null!;
    public string SmtpUser { get; set; } = null!;
    public string SmtpPassword { get; set; } = null!;

    [ConfigurationKeyName("Email_Port")]
    public int Port { get; set; }

    public bool UseSsl {get; set; }
    public int EmailDelaySeconds { get; set; }
}