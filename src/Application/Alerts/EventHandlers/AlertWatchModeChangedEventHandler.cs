using Domain.Events.Alerts;
using MediatR;
using SiteWatcher.Application.Interfaces;

namespace SiteWatcher.Application.Alerts.EventHandlers;

public class AlertWatchModeChangedEventHandler : INotificationHandler<AlertWatchModeChangedEvent>
{
    private readonly IAlertRepository _alertRepository;

    public AlertWatchModeChangedEventHandler(IAlertRepository alertRepository)
    {
        _alertRepository = alertRepository;
    }

    public Task Handle(AlertWatchModeChangedEvent notification, CancellationToken cancellationToken)
    {
        _alertRepository.DeleteWatchMode(notification.OldWatchModeId);
        return Task.CompletedTask;
    }
}