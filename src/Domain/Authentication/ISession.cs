using SiteWatcher.Domain.Common.Services;
using SiteWatcher.Domain.Common.ValueObjects;
using SiteWatcher.Domain.Users.Enums;

namespace SiteWatcher.Domain.Authentication;

public interface ISession
{
    public DateTime Now { get; }
    public UserId UserId { get;}
    public string Email { get;}
    public bool EmailConfirmed { get;}
    public string Name { get; }
    public Language Language { get; }
    public IEnumerable<string> SessionIds { get; }

    Task Load(ICache cache, string userId);
}