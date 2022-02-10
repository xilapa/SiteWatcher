namespace SiteWatcher.Domain.Entities;

public abstract class BaseEntity<IdType>
{
    public IdType Id { get; set; }
}