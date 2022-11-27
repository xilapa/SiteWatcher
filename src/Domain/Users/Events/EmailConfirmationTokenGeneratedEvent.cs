using SiteWatcher.Domain.Common.Events;
using SiteWatcher.Domain.Common.ValueObjects;
using SiteWatcher.Domain.Users.Enums;

namespace SiteWatcher.Domain.Users.Events;

public class EmailConfirmationTokenGeneratedEvent : BaseEvent
{
    public EmailConfirmationTokenGeneratedEvent(User user)
    {
        UserId = user.Id;
        Name = user.Name;
        Email = user.Email;
        Language = user.Language;
        ConfirmationToken = user.SecurityStamp!;
    }

    public UserId UserId { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public Language Language { get; set; }
    public string ConfirmationToken { get; set; }
}