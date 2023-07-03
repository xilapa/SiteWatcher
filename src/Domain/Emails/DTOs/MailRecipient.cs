using SiteWatcher.Domain.Common.ValueObjects;

namespace SiteWatcher.Domain.Emails.DTOs;

public class MailRecipient
{
    public MailRecipient(string name, string email, UserId userId)
    {
        Name = name;
        Email = email;
        UserId = userId;
    }

    public string Name { get; set; }
    public string Email { get; set; }
    public UserId UserId { get; set; }
}