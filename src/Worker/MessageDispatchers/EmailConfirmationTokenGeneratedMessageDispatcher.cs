using MassTransit;
using SiteWatcher.Application.Common.Messages;
using SiteWatcher.Domain.Users.Messages;

namespace Worker.MessageDispatchers;

public class EmailConfirmationTokenGeneratedMessageDispatcher : IConsumer<EmailConfirmationTokenGeneratedMessage>
{
    private readonly IMessageHandler<EmailConfirmationTokenGeneratedMessage> _handler;

    public EmailConfirmationTokenGeneratedMessageDispatcher(IMessageHandler<EmailConfirmationTokenGeneratedMessage> handler)
    {
        _handler = handler;
    }

    public async Task Consume(ConsumeContext<EmailConfirmationTokenGeneratedMessage> context)
    {
        await _handler.Handle(context.Message, context.CancellationToken);
    }
}