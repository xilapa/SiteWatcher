using MediatR;
using SiteWatcher.Domain.Alerts.Events;
using SiteWatcher.Domain.Alerts.Repositories;

namespace SiteWatcher.Application.Alerts.EventHandlers;

public class AlertRuleChangedEventHandler : INotificationHandler<AlertRuleChangedEvent>
{
    private readonly IAlertRepository _alertRepository;

    public AlertRuleChangedEventHandler(IAlertRepository alertRepository)
    {
        _alertRepository = alertRepository;
    }

    public Task Handle(AlertRuleChangedEvent notification, CancellationToken cancellationToken)
    {
        _alertRepository.DeleteRule(notification.OldRuleId);
        return Task.CompletedTask;
    }
}