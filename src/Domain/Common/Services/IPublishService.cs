namespace SiteWatcher.Domain.Common.Services;

public interface IPublishService
{
    Task WithPublisher(Func<IPublisher, Task> func, CancellationToken ct);
}

public interface IPublisher
{
    Task PublishAsync(string routingKey, object message, Dictionary<string, string>? headers, CancellationToken ct);
    Task PublishAsync(string routingKey, object message, CancellationToken ct);
}