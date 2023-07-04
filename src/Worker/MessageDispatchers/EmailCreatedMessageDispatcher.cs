using DotNetCore.CAP;
using SiteWatcher.Application.Common.Messages;
using SiteWatcher.Domain.Emails.Events;

namespace SiteWatcher.Worker.MessageDispatchers;

public sealed class EmailCreatedMessageDispatcher : ICapSubscribe
{
    private readonly IMessageHandler<EmailCreatedMessage> _handler;

    public EmailCreatedMessageDispatcher(IMessageHandler<EmailCreatedMessage> handler)
    {
        _handler = handler;
    }

    // CAP uses this attribute to create a queue and bind it with a routing key.
    // The message name is the routing key and group name is used to create the queue name.
    // Cap append the version on the queue name (e.g., queue-name.v1)
    [CapSubscribe(nameof(EmailCreatedMessage), Group = nameof(EmailCreatedMessage))]
    public async Task Dispatch(EmailCreatedMessage msg, CancellationToken cancellationToken)
    {
        await _handler.Handle(msg, cancellationToken);
    }
}