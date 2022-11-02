using SiteWatcher.Domain.Models.Common;

namespace SiteWatcher.Domain.Models.Emails;

public class Email
{
    // ctor for EF
    protected Email()
    {
        Recipients = new List<EmailRecipient>();
    }

    public Email(DateTime currentTime, string body, string subject, string? error) : this()
    {
        Id = new EmailId();
        DateSent = currentTime;
        Subject = subject;
        Body = body;
        ErrorMessage = error;
    }

    public EmailId Id { get; set; }
    public List<EmailRecipient> Recipients { get; set; }
    public DateTime DateSent { get; set; }
    public string Subject { get; set; }
    public string Body { get; set; }
    public string? ErrorMessage { get; set; }
}

public class EmailRecipient
{
    public string Name { get; set; }
    public string Email { get; set; }
}