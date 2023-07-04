namespace SiteWatcher.Domain.Common.Services;

public interface IPublisher
{
    Task PublishAsync(string routingKey, object message, CancellationToken ct);
}