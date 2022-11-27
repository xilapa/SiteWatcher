using MediatR;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Domain.Alerts.Events;
using SiteWatcher.Domain.Common.Constants;

namespace SiteWatcher.Application.Alerts.EventHandlers;

public class AlertsChangedEventHandler : INotificationHandler<AlertsChangedEvent>
{
    private readonly IFireAndForgetService _fireAndForgetService;

    public AlertsChangedEventHandler(IFireAndForgetService fireAndForgetService)
    {
        _fireAndForgetService = fireAndForgetService;
    }

    public Task Handle(AlertsChangedEvent notification, CancellationToken cancellationToken)
    {
        _fireAndForgetService.ExecuteWith<ICache>(async cache =>
        {
            await cache.DeleteKeyAsync(CacheKeys.UserAlerts(notification.UserId));
            await cache.DeleteKeyAsync(CacheKeys.UserAlertSearch(notification.UserId));
        });
        return Task.CompletedTask;
    }
}