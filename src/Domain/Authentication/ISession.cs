using SiteWatcher.Domain.Common.ValueObjects;

namespace SiteWatcher.Domain.Authentication;

public interface ISession
{
    public DateTime Now { get; }
    public UserId? UserId { get;}
    public string AuthTokenPayload { get; }
}