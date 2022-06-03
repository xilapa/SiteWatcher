using SiteWatcher.Application.Interfaces;

namespace SiteWatcher.WebAPI.Settings;

public class EmailSettings : IEmailSettings
{
    public string FromEmail { get; set; }
    public string FromName { get; set; }
    public string SmtpHost { get; set; }
    public string IAMUserName { get; set; }
    public string SmtpUser { get; set; }
    public string SmtpPassword { get; set; }
    public int TLSPort { get; set; }
}