namespace SiteWatcher.Domain.Models;

public class IdempotentConsumer
{
    public string MessageId { get; set; }
    public string Consumer { get; set; }
}