using Domain.Events;

namespace SiteWatcher.Domain.Models.Common;

public abstract class BaseModel<IdType> : IBaseModel
{
    // ctor for EF
    protected BaseModel()
    {
        _domainEvents = new List<BaseEvent>();
    }

    protected BaseModel(IdType id, DateTime currentDate) : this()
    {
        Id = id;
        Active = true;
        CreatedAt = currentDate;
        LastUpdatedAt = currentDate;
    }

    public IdType Id { get; }
    public bool Active { get; protected set; }
    public DateTime CreatedAt { get; protected set;}
    public DateTime LastUpdatedAt { get; protected set; }

    #region Domain Events

    private readonly List<BaseEvent> _domainEvents;
    public BaseEvent[] DomainEvents => _domainEvents.ToArray();
    public void AddDomainEvent(BaseEvent domainEvent) => _domainEvents.Add(domainEvent);
    public void RemoveDomainEvent(BaseEvent domainEvent) => _domainEvents.Remove(domainEvent);
    public void ClearDomainEvents() => _domainEvents.Clear();

    #endregion
}

public interface IBaseModel
{
    BaseEvent[] DomainEvents { get; }
    void AddDomainEvent(BaseEvent domainEvent);
    void RemoveDomainEvent(BaseEvent domainEvent);
    void ClearDomainEvents();
}