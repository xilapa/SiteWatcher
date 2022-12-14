using SiteWatcher.Domain.Common.ValueObjects;

namespace SiteWatcher.Domain.Emails;

public class MailMessage
{
    public MailMessage()
    {
        Recipients = Array.Empty<MailRecipient>();
    }

    public MailMessage(EmailId? emailId, string subject, string? body, bool htmlBody, params MailRecipient[] recipients) : this()
    {
        Subject = subject;
        Body = body;
        HtmlBody = htmlBody;
        Recipients = recipients;
        EmailId = emailId;
    }

    /// <summary>
    /// The related Email entity Id
    /// </summary>
    public EmailId? EmailId { get; set; }
    public string Subject { get; set; } = null!;
    public string? Body { get; set; }
    public bool HtmlBody { get; set; }
    public MailRecipient[] Recipients { get; set; }
}