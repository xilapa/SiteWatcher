using SiteWatcher.Domain.Common;
using SiteWatcher.Domain.Common.ValueObjects;
using SiteWatcher.Domain.Emails.DTOs;
using SiteWatcher.Domain.Emails.Events;
using SiteWatcher.Domain.Users;

namespace SiteWatcher.Domain.Emails;

public class Email : BaseModel<EmailId>
{
    // ctor for EF
    protected Email()
    {  }

    public Email(string body, bool htmlBody, string subject, MailRecipient recipient, DateTime currentDate) :
        base(EmailId.New(), currentDate)
    {
        Subject = subject;
        Body = body;
        Recipient = $"{recipient.Name}:{recipient.Email}";
        UserId = recipient.UserId;
        var emailMessage = new MailMessage
        {
            Subject = subject,
            Body = body,
            HtmlBody = htmlBody,
            Recipients = new[] { recipient },
            EmailId = Id
        };
        AddDomainEvent(new EmailCreatedEvent(emailMessage));
    }

    public Email(string body, bool htmlBody, string subject, User user, DateTime currentDate) :
        base(EmailId.New(), currentDate)
    {
        Subject = subject;
        Body = body;
        Recipient = $"{user.Name}:{user.Email}";
        UserId = user.Id;
        var emailMessage = new MailMessage
        {
            Subject = subject,
            Body = body,
            HtmlBody = htmlBody,
            Recipients = new[] { new MailRecipient(user.Name, user.Email, user.Id) },
            EmailId = Id
        };
        AddDomainEvent(new EmailCreatedEvent(emailMessage));
    }

    public string Recipient { get; }
    public DateTime? DateSent { get; private set; }
    public string Subject { get; }
    public string Body { get; }
    public string? ErrorMessage { get; private set; }

    public DateTime? ErrorDate { get; private set; }
    public UserId UserId { get; }

    public void MarkAsSent(DateTime currentDate)
    {
        DateSent = currentDate;
        LastUpdatedAt = currentDate;
    }

    public void MarkAsFailed(string errorMessage, DateTime currentDate)
    {
        ErrorMessage = errorMessage;
        ErrorDate = currentDate;
        LastUpdatedAt = currentDate;
    }
}