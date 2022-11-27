using SiteWatcher.Domain.Common.ValueObjects;

namespace SiteWatcher.Domain.Emails;

public class Email
{
    // ctor for EF
    protected Email()
    {
        Recipients = new List<MailRecipient>();
    }

    public Email(string body, string subject, params MailRecipient[] recipients)
    {
        Id = EmailId.New();
        Subject = subject;
        Body = body;
        Recipients = recipients.ToList();
    }

    public EmailId Id { get; set; }
    public List<MailRecipient> Recipients { get; set; }
    public DateTime? DateSent { get; set; }
    public string Subject { get; set; } = null!;
    public string? Body { get; set; }
    public string? ErrorMessage { get; set; }

    public bool HasSent()
    {
        if (!DateSent.HasValue)
            return false;

        if (!string.IsNullOrEmpty(ErrorMessage))
            return false;

        return true;
    }
}