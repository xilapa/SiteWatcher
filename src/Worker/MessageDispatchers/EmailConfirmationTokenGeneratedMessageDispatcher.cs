using DotNetCore.CAP;
using SiteWatcher.Application.Common.Messages;
using SiteWatcher.Domain.Users.Messages;

namespace Worker.MessageDispatchers;

public class EmailConfirmationTokenGeneratedMessageDispatcher : ICapSubscribe
{
    private readonly IMessageHandler<EmailConfirmationTokenGeneratedMessage> _handler;

    public EmailConfirmationTokenGeneratedMessageDispatcher(IMessageHandler<EmailConfirmationTokenGeneratedMessage> handler)
    {
        _handler = handler;
    }

    [CapSubscribe(nameof(EmailConfirmationTokenGeneratedMessage), Group = nameof(EmailConfirmationTokenGeneratedMessage))]
    public async Task Dispatch(EmailConfirmationTokenGeneratedMessage message, CancellationToken ct)
    {
        await _handler.Handle(message, ct);
    }
}