using DotNetCore.CAP;
using SiteWatcher.Application.Common.Messages;
using SiteWatcher.Domain.Alerts.Messages;

namespace SiteWatcher.Worker.Consumers;

public class AlertsTriggeredMessageDispatcher : ICapSubscribe
{
    private readonly IMessageHandler<AlertsTriggeredMessage> _handler;

    public AlertsTriggeredMessageDispatcher(IMessageHandler<AlertsTriggeredMessage> handler)
    {
        _handler = handler;
    }

    [CapSubscribe(nameof(AlertsTriggeredMessage), Group = nameof(AlertsTriggeredMessage))]
    public async Task Dispatch(AlertsTriggeredMessage message, CancellationToken ct)
    {
        await _handler.Handle(message, ct);
    }
}