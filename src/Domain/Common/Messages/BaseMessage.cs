using SiteWatcher.Domain.Common.Events;

namespace SiteWatcher.Domain.Common.Messages;

public abstract class BaseMessage : BaseEvent
{
    public string Id { get; set; }
}