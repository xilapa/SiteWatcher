using MassTransit;
using Mediator;
using SiteWatcher.Domain.Common.Messages;

namespace SiteWatcher.Application.Common.Messages;

public class MessagePublisher : INotificationHandler<BaseMessage>
{
    private readonly IPublishEndpoint _publisher;

    public MessagePublisher(IPublishEndpoint publisher)
    {
        _publisher = publisher;
    }

    public async ValueTask Handle(BaseMessage notification, CancellationToken cancellationToken)
    {
        await _publisher.Publish(notification, notification.GetType(), cancellationToken);
    }
}