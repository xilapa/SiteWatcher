namespace SiteWatcher.Domain.Models;

public abstract class BaseModel<IdType>
{
    protected BaseModel()
    {
        Active = true;
        CreatedAt = new DateTime(DateTime.UtcNow.Ticks);
        LastUpdatedAt = new DateTime(DateTime.UtcNow.Ticks);
    }

    public IdType Id { get; protected set; }
    public bool Active { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime LastUpdatedAt { get; private set; }

}