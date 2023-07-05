using MassTransit;
using SiteWatcher.Application.Common.Messages;
using SiteWatcher.Domain.Emails.Events;

namespace SiteWatcher.Worker.MessageDispatchers;

public sealed class EmailCreatedMessageDispatcher : IConsumer<EmailCreatedMessage>
{
    private readonly IMessageHandler<EmailCreatedMessage> _handler;

    public EmailCreatedMessageDispatcher(IMessageHandler<EmailCreatedMessage> handler)
    {
        _handler = handler;
    }

    public async Task Consume(ConsumeContext<EmailCreatedMessage> context)
    {
        await _handler.Handle(context.Message, context.CancellationToken);
    }
}