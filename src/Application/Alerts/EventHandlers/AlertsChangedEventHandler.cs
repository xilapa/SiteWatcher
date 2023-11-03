using SiteWatcher.Application.Common.Command;
using SiteWatcher.Domain.Alerts.Events;
using SiteWatcher.Domain.Common.Constants;
using SiteWatcher.Domain.Common.Services;

namespace SiteWatcher.Application.Alerts.EventHandlers;

public class AlertsChangedEventHandler : IApplicationHandler
{
    private readonly ICache _cache;

    public AlertsChangedEventHandler(ICache cache)
    {
        _cache = cache;
    }

    public async ValueTask Handle(AlertsChangedEvent notification, CancellationToken cancellationToken)
    {
        await _cache.DeleteKeyAsync(CacheKeys.UserAlerts(notification.UserId));
        await _cache.DeleteKeyAsync(CacheKeys.UserAlertSearch(notification.UserId));
    }
}