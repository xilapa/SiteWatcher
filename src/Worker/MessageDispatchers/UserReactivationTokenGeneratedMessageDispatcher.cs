using MassTransit;
using SiteWatcher.Application.Common.Messages;
using SiteWatcher.Domain.Users.Messages;

namespace Worker.MessageDispatchers;

public class UserReactivationTokenGeneratedMessageDispatcher : IConsumer<UserReactivationTokenGeneratedMessage>
{
    private readonly IMessageHandler<UserReactivationTokenGeneratedMessage> _handler;

    public UserReactivationTokenGeneratedMessageDispatcher(IMessageHandler<UserReactivationTokenGeneratedMessage> handler)
    {
        _handler = handler;
    }

    public async Task Consume(ConsumeContext<UserReactivationTokenGeneratedMessage> context)
    {
        await _handler.Handle(context.Message, context.CancellationToken);
    }
}