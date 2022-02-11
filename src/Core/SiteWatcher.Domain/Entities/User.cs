using System;

namespace SiteWatcher.Domain.Entities;

public class User : BaseEntity<Guid>
{
    public User(string name, string email)
    {
        Name = name;
        Email = email;
    }

    public string Name { get; set; }
    public string Email { get; set; }
    public bool EmailConfirmed { get; set; } = false;
    public DateTime? EmailConfirmedAt { get; set; }
    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
    public bool Subscribed { get; set; } = false;
    public string SecurityStamp { get; set; }

    // TODO: separar em entidade diferente
    public DateTime? SubscribedAt { get; set; }  
    public DateTime? UnsubscribedAt { get; set; }
}