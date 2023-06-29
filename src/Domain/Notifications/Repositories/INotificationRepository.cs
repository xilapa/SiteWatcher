namespace SiteWatcher.Domain.Notifications.Repositories;

public interface INotificationRepository
{
    public void Add(Notification notification);
}