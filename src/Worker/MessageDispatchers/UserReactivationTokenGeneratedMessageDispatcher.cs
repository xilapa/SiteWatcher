using DotNetCore.CAP;
using SiteWatcher.Application.Common.Messages;
using SiteWatcher.Domain.Users.Messages;

namespace Worker.MessageDispatchers;

public class UserReactivationTokenGeneratedMessageDispatcher : ICapSubscribe
{
    private readonly IMessageHandler<UserReactivationTokenGeneratedMessage> _handler;

    public UserReactivationTokenGeneratedMessageDispatcher(IMessageHandler<UserReactivationTokenGeneratedMessage> handler)
    {
        _handler = handler;
    }

    [CapSubscribe(nameof(UserReactivationTokenGeneratedMessage), Group = nameof(UserReactivationTokenGeneratedMessage))]
    public async Task Dispatch(UserReactivationTokenGeneratedMessage message, CancellationToken ct)
    {
        await _handler.Handle(message, ct);
    }
}