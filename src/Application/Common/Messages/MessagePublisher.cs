using Mediator;
using SiteWatcher.Domain.Common.Messages;
using IPublisher = SiteWatcher.Domain.Common.Services.IPublisher;

namespace SiteWatcher.Application.Common.Messages;

public class MessagePublisher : INotificationHandler<BaseMessage>
{
    private readonly IPublisher _publisher;

    public MessagePublisher(IPublisher publisher)
    {
        _publisher = publisher;
    }

    public async ValueTask Handle(BaseMessage notification, CancellationToken cancellationToken)
    {
        await _publisher.PublishAsync(notification, cancellationToken);
    }
}