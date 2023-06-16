using SiteWatcher.Domain.Common.Events;
using SiteWatcher.Domain.Emails.DTOs;

namespace SiteWatcher.Domain.Emails.Events;

public sealed class EmailCreatedEvent : BaseEvent
{
    public EmailCreatedEvent(MailMessage mailMessage)
    {
        MailMessage = mailMessage;
    }

    public MailMessage MailMessage { get; set; }
}