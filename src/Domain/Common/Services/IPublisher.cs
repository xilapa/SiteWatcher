namespace SiteWatcher.Domain.Common.Services;

public interface IPublisher
{
    Task PublishAsync(string routingKey, object message, Dictionary<string, string>? headers, CancellationToken ct);
    Task PublishAsync(string routingKey, object message, CancellationToken ct);
}