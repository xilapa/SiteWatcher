namespace SiteWatcher.Domain.Common.ValueObjects;

public class IdempotentConsumer
{
    public string MessageId { get; set; } = null!;
    public string Consumer { get; set; } = null!;
}