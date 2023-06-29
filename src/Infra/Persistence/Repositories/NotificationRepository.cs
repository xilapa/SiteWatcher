using SiteWatcher.Domain.Notifications;
using SiteWatcher.Domain.Notifications.Repositories;
using SiteWatcher.Infra;

namespace Infra.Persistence.Repositories;

public class NotificationRepository : INotificationRepository
{
    private readonly SiteWatcherContext _ctx;

    public NotificationRepository(SiteWatcherContext ctx)
    {
        _ctx = ctx;
    }

    public void Add(Notification notification) => _ctx.Add(notification);
}