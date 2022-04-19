namespace SiteWatcher.Domain.Models;

public abstract class BaseModel<IdType>
{
    protected BaseModel(IdType id)
    {
        Id = id;
        Active = true;
        CreatedAt = new DateTime(DateTime.UtcNow.Ticks);
        LastUpdatedAt = new DateTime(DateTime.UtcNow.Ticks);
    }

    public IdType Id { get; }
    public bool Active { get; }
    public DateTime CreatedAt { get; }
    public DateTime LastUpdatedAt { get; }
}