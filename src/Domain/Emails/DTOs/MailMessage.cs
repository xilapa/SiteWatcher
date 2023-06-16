using SiteWatcher.Domain.Common.ValueObjects;

namespace SiteWatcher.Domain.Emails.DTOs;

public class MailMessage
{
    public MailMessage()
    {
        Recipients = Array.Empty<MailRecipient>();
    }

    /// <summary>
    /// The related Email entity Id
    /// </summary>
    public EmailId? EmailId { get; set; }
    public string Subject { get; set; }
    public string? Body { get; set; }
    public bool HtmlBody { get; set; }
    public MailRecipient[] Recipients { get; set; }
}