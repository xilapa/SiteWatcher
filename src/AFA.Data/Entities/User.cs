using System;

namespace AFA.Data.Entities;

public class User
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public bool EmailConfirmed { get; set; }
    public bool Subscribed { get; set; }
    public string SecurityStamp { get; set; }
}