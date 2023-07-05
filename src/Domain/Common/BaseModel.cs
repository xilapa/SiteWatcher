using System.Collections.ObjectModel;
using SiteWatcher.Domain.Common.Events;

namespace SiteWatcher.Domain.Common;

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

    public IdType Id { get; protected set; }
    public bool Active { get; protected set; }
    public DateTime CreatedAt { get; protected set;}
    public DateTime LastUpdatedAt { get; protected set; }

    #region Domain Events

    private readonly List<BaseEvent> _domainEvents;
    public ReadOnlyCollection<BaseEvent> DomainEvents => _domainEvents.AsReadOnly();
    public void AddDomainEvent(BaseEvent domainEvent) => _domainEvents.Add(domainEvent);
    public void ClearDomainEvents() => _domainEvents.Clear();

    #endregion
}

public interface IBaseModel
{
    ReadOnlyCollection<BaseEvent> DomainEvents { get; }
    void ClearDomainEvents();
}