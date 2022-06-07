using SiteWatcher.Domain.Enums;
using SiteWatcher.Domain.Models;
using SiteWatcher.Domain.Models.Common;

namespace Domain.Events;

public class UserReactivationTokenGeneratedEvent : BaseEvent
{
    public UserReactivationTokenGeneratedEvent(User user)
    {
        UserId = user.Id;
        Name = user.Name;
        Email = user.Email;
        Language = user.Language;
        ConfirmationToken = user.SecurityStamp;
    }

    public UserId UserId { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public ELanguage Language { get; set; }
    public string ConfirmationToken { get; set; }
}