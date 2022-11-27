namespace SiteWatcher.Domain.Emails;

public class MailMessage
{
    public MailMessage()
    {
        Recipients = Array.Empty<MailRecipient>();
    }

    public string Subject { get; set; } = null!;
    public string? Body { get; set; }
    public bool HtmlBody { get; set; }
    public MailRecipient[] Recipients { get; set; }
}