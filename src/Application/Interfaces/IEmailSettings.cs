namespace SiteWatcher.Application.Interfaces;

public interface IEmailSettings
{
    public string FromEmail { get; set; }
    public string FromName { get; set; }
    public string ReplyToEmail { get; set; }
    public string ReplyToName { get; set; }
    public string SmtpHost { get; set; }
    public string SmtpUser { get; set; }
    public string SmtpPassword { get; set; }
    public int Port { get; set; }
    public bool UseTls { get; set; }
    public int EmailDelaySeconds { get; set; }
}