using SiteWatcher.Domain.Emails;

namespace SiteWatcher.Worker.Messaging;

public sealed class EmailNotificationMessage
{
    // ctor for json serializer
    public EmailNotificationMessage()
    { }

    public EmailNotificationMessage(MailMessage mailMessage)
    {
        Subject = mailMessage.Subject;
        Body = mailMessage.Body!;
        Recipients = mailMessage.Recipients;
    }

    public string Subject{ get; set; }
    public string Body{ get; set; }
    public MailRecipient[] Recipients { get; set; }
}