using SiteWatcher.Domain.Common.Messages;

namespace SiteWatcher.Application.Common.Messages;

public interface IMessageHandler<T> where T : BaseMessage
{
    Task Handle(T message, CancellationToken ct);
}