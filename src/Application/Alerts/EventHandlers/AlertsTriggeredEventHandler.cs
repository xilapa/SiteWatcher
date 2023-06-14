using MediatR;
using SiteWatcher.Application.Common.Constants;
using SiteWatcher.Domain.Alerts.Events;
using SiteWatcher.Domain.Common.Services;

namespace SiteWatcher.Application.Alerts.EventHandlers;

public class AlertsTriggeredEventHandler : INotificationHandler<AlertsChangedEvent>
{
    private readonly IPublishService _pubService;

    public AlertsTriggeredEventHandler(IPublishService pubService)
    {
        _pubService = pubService;
    }

    public async Task Handle(AlertsChangedEvent notification, CancellationToken cancellationToken)
    {
        await _pubService.WithPublisher(async publisher =>
                await publisher.PublishAsync(RoutingKeys.AlertsTriggered, notification, cancellationToken)
        , cancellationToken);
    }
}