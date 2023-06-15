using SiteWatcher.Domain.Common.ValueObjects;

namespace SiteWatcher.Domain.Emails;

public class Email
{
    // ctor for EF
    protected Email()
    {  }

    public Email(string body, string subject, MailRecipient recipient)
    {
        Id = EmailId.New();
        Subject = subject;
        Body = body;
        Recipient = $"{recipient.Name}:{recipient.Email}";
        UserId = recipient.UserId;
    }

    public EmailId Id { get; set; }
    public string Recipient { get; set; } = null!;
    public DateTime? DateSent { get; set; }
    public string Subject { get; set; } = null!;
    public string Body { get; set; }
    public string? ErrorMessage { get; set; }
    public UserId UserId { get; set; }
    // public ICollection<Alert> Alerts { get; set; }

    public bool HasSent()
    {
        if (!DateSent.HasValue)
            return false;

        if (!string.IsNullOrEmpty(ErrorMessage))
            return false;

        return true;
    }
}