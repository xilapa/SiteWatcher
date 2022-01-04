using System;

namespace AFA.Data.Entities;

public class User
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public bool EmailConfirmed { get; set; } = false;
    public bool Subscribed { get; set; } = false;
    public string SecurityStamp { get; set; }
    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
    public DateTime? EmailConfirmedAt { get; set; }
    public DateTime? UnsubscribedAt { get; set; }  
}