
namespace SiteWatcher.Domain.Common.Services;

public interface IPublisher
{
    Task PublishAsync(object message, CancellationToken ct);
}