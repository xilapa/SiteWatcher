using MassTransit;
using SiteWatcher.Application.Common.Messages;
using SiteWatcher.Domain.Alerts.Messages;

namespace SiteWatcher.Worker.Consumers;

public class AlertsTriggeredMessageDispatcher : IConsumer<AlertsTriggeredMessage>
{
    private readonly IMessageHandler<AlertsTriggeredMessage> _handler;

    public AlertsTriggeredMessageDispatcher(IMessageHandler<AlertsTriggeredMessage> handler)
    {
        _handler = handler;
    }

    public async Task Consume(ConsumeContext<AlertsTriggeredMessage> context)
    {
        await _handler.Handle(context.Message, context.CancellationToken);
    }
}