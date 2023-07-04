using SiteWatcher.Domain.Common.Messages;
using SiteWatcher.Domain.Common.ValueObjects;
using SiteWatcher.Domain.Emails.DTOs;

namespace SiteWatcher.Domain.Emails.Events;

public sealed class EmailCreatedMessage : BaseMessage
{
    public EmailCreatedMessage(EmailId emailId, string subject, string? body, bool htmlBody, MailRecipient recipient,
        DateTime currentTime)
    {
        Id = $"{emailId}:{currentTime.Ticks}";
        EmailId = emailId;
        Subject = subject;
        Body = body;
        HtmlBody = htmlBody;
        Recipients = new[]{recipient};
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