using Mediator;
using SiteWatcher.Application.Common.Constants;
using SiteWatcher.Domain.Alerts.Events;
using IPublisher = SiteWatcher.Domain.Common.Services.IPublisher;

namespace SiteWatcher.Application.Alerts.EventHandlers;

public class AlertsTriggeredEventHandler : INotificationHandler<AlertsTriggeredEvent>
{
    private readonly IPublisher _pubService;

    public AlertsTriggeredEventHandler(IPublisher pubService)
    {
        _pubService = pubService;
    }

    public async ValueTask Handle(AlertsTriggeredEvent notification, CancellationToken cancellationToken)
    {
        await _pubService.PublishAsync(RoutingKeys.AlertsTriggered, notification, cancellationToken);
    }
}