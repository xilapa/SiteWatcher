using MediatR;
using SiteWatcher.Domain.Enums;

namespace SiteWatcher.Application.Notifications;

public class UserRegisteredNotification : INotification
{
    public string Name { get; set; }
    public string Email { get; set; }
    public ELanguage Language { get; set; }
}