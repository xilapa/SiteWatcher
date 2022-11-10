namespace SiteWatcher.Domain.Models.Worker;

public class IdempotentConsumer
{
    public string MessageId { get; set; } = null!;
    public string Consumer { get; set; } = null!;
}