using System;
using SiteWatcher.Domain.Enums;

namespace SiteWatcher.Domain.Models;

public class User : BaseModel<Guid>
{
    public string GoogleId { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public bool EmailConfirmed { get; set; }
    public ELanguage Language { get; set; }
    public string SecurityStamp { get; set; }
}