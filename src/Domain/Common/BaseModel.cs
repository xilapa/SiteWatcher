using System.Collections.ObjectModel;
using SiteWatcher.Domain.Common.Events;
using SiteWatcher.Domain.Common.Messages;

namespace SiteWatcher.Domain.Common;

public abstract class BaseModel<IdType> : IBaseModel
{
    // ctor for EF
    protected BaseModel()
    {
        _domainEvents = new List<BaseEvent>();
        _messages = new List<BaseMessage>();
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

    #region Messages

    private readonly List<BaseMessage> _messages;
    public ReadOnlyCollection<BaseMessage> Messages => _messages.AsReadOnly();
    public void AddMessage(BaseMessage message) => _messages.Add(message);
    public void ClearMessages() => _messages.Clear();

    #endregion
}

public interface IBaseModel
{
    ReadOnlyCollection<BaseEvent> DomainEvents { get; }
    void ClearDomainEvents();
    ReadOnlyCollection<BaseMessage> Messages { get; }
    void ClearMessages();
}