namespace SiteWatcher.Application.Interfaces;

public interface IEmailSettings
{
    public string FromEmail { get; set; }
    public string FromName { get; set; }
    public string SmtpHost { get; set; }
    public string SmtpUser { get; set; }
    public string SmtpPassword { get; set; }
    public int TLSPort { get; set; }
}