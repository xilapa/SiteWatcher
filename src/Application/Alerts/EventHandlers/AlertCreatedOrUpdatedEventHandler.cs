using Domain.Events.Alerts;
using MediatR;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Domain.Utils;

namespace SiteWatcher.Application.Alerts.EventHandlers;

public class AlertCreatedOrUpdatedEventHandler : INotificationHandler<AlertCreatedOrUpdatedEvent>
{
    private readonly IFireAndForgetService _fireAndForgetService;

    public AlertCreatedOrUpdatedEventHandler(IFireAndForgetService fireAndForgetService)
    {
        _fireAndForgetService = fireAndForgetService;
    }

    public Task Handle(AlertCreatedOrUpdatedEvent notification, CancellationToken cancellationToken)
    {
        _fireAndForgetService.ExecuteWith<ICache>(cache =>
            cache.DeleteKeyAsync(CacheKeys.UserAlerts(notification.UserId)));
        return Task.CompletedTask;
    }
}