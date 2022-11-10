namespace SiteWatcher.Domain.Models.Emails;

public struct MailRecipient
{
    public MailRecipient(string name, string email)
    {
        Name = name;
        Email = email;
    }

    public string Name { get; set; }
    public string Email { get; set; }
}