using SiteWatcher.Domain.Models.Emails;

namespace SiteWatcher.Worker.Messaging;

public sealed class EmailNotificationMessage
{
    public EmailNotificationMessage(string subject, string body, params MailRecipient[] recipients)
    {
        Subject = subject;
        Body = body;
        Recipients = recipients;
    }

    public string Subject{ get; set; }
    public string Body{ get; set; }
    public MailRecipient[] Recipients { get; set; }
}