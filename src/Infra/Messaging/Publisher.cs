using DotNetCore.CAP;
using SiteWatcher.Domain.Common.Services;

namespace SiteWatcher.Infra.Messaging;

public sealed class Publisher : IPublisher
{
    private readonly ICapPublisher _capPublisher;

    public Publisher(ICapPublisher capPublisher)
    {
        _capPublisher = capPublisher;
    }

    public async Task PublishAsync(string routingKey, object message, CancellationToken ct)
    {
        await _capPublisher.PublishAsync(routingKey, message, cancellationToken: ct);
    }
}