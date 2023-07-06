using SiteWatcher.Domain.Common;
using SiteWatcher.Domain.Common.ValueObjects;
using SiteWatcher.Domain.Emails.DTOs;
using SiteWatcher.Domain.Emails.Messages;
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
        AddDomainEvent(new EmailCreatedMessage(Id,subject, body, htmlBody, recipient, currentDate));
    }

    public Email(string body, bool htmlBody, string subject, User user, DateTime currentDate) :
        base(EmailId.New(), currentDate)
    {
        Subject = subject;
        Body = body;
        Recipient = $"{user.Name}:{user.Email}";
        UserId = user.Id;
        var recipient = new MailRecipient(user.Name, user.Email, user.Id);
        AddDomainEvent(new EmailCreatedMessage(Id,subject, body, htmlBody, recipient, currentDate));
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