using System;

namespace SiteWatcher.Domain.Models;

public abstract class BaseModel<IdType>
{
    public IdType Id { get; set; }
    public bool Active { get; set; }
    public DateTime CreatedAt { get; set; }
}