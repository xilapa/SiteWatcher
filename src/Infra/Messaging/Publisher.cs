using MassTransit;
using SiteWatcher.Domain.Common.Services;

namespace SiteWatcher.Infra.Messaging;

public sealed class Publisher : IPublisher
{
    private readonly IPublishEndpoint _publishEndpoint;

    public Publisher(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
    }

    public async Task PublishAsync(object message, CancellationToken ct)
    {
        await _publishEndpoint.Publish(message, ct);
    }
}