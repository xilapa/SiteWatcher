namespace SiteWatcher.Domain.Common;

public abstract class BaseModel<IdType>
{
    protected BaseModel()
    { }

    protected BaseModel(IdType id, DateTime currentDate)
    {
        Id = id;
        Active = true;
        CreatedAt = currentDate;
        LastUpdatedAt = currentDate;
    }

    public IdType Id { get; protected set; }
    public bool Active { get; protected set; }
    public DateTime CreatedAt { get; protected set;}
    public DateTime LastUpdatedAt { get; protected set; }
}