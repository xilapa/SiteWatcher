using Mediator;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Domain.Alerts.Events;
using SiteWatcher.Domain.Common.Constants;
using SiteWatcher.Domain.Common.Services;

namespace SiteWatcher.Application.Alerts.EventHandlers;

public class AlertsChangedEventHandler : INotificationHandler<AlertsChangedEvent>
{
    private readonly IFireAndForgetService _fireAndForgetService;

    public AlertsChangedEventHandler(IFireAndForgetService fireAndForgetService)
    {
        _fireAndForgetService = fireAndForgetService;
    }

    public ValueTask Handle(AlertsChangedEvent notification, CancellationToken cancellationToken)
    {
        _fireAndForgetService.ExecuteWith<ICache>(async cache =>
        {
            await cache.DeleteKeyAsync(CacheKeys.UserAlerts(notification.UserId));
            await cache.DeleteKeyAsync(CacheKeys.UserAlertSearch(notification.UserId));
        });
        return ValueTask.CompletedTask;
    }
}