using Domain.Events;

namespace SiteWatcher.Domain.Models.Common;

public abstract class BaseModel<IdType> : IBaseModel
{
    protected BaseModel(IdType id)
    {
        Id = id;
        Active = true;
        // TODO: Tirar esse datetime now cravado aqui
        CreatedAt = new DateTime(DateTime.UtcNow.Ticks);
        LastUpdatedAt = new DateTime(DateTime.UtcNow.Ticks);
        _domainEvents = new List<BaseEvent>();
    }

    public IdType Id { get; }
    public bool Active { get; protected set; }
    public DateTime CreatedAt { get; }
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