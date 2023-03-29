using SiteWatcher.Domain.Common.ValueObjects;
using SiteWatcher.Domain.Users.Enums;

namespace SiteWatcher.Domain.Authentication;

public interface ISession
{
    public DateTime Now { get; }
    public UserId? UserId { get;}
    public string? Email { get;}
    public string? GoogleId { get; }
    public string? UserName { get; }
    public Language? Language { get; }
    public string AuthTokenPayload { get; }
}