using SiteWatcher.Domain.Common.Constants;
using SiteWatcher.Domain.Common.Services;
using SiteWatcher.Domain.Common.ValueObjects;
using SiteWatcher.Domain.Users.Enums;

namespace SiteWatcher.Domain.Authentication;

public class Session : ISession
{
    public Session()
    { }

    private Session(AuthSession authSession)
    {
        UserId = authSession.UserId!.Value;
        Email = authSession.Email!;
        EmailConfirmed = authSession.EmailConfirmed;
        Name = authSession.Name!;
        Language = authSession.Language!.Value;
        SessionIds = new List<string>();
    }

    public static Session Create(AuthSession authSession)
    {
        return new Session(authSession);
    }

    public virtual DateTime Now => DateTime.UtcNow;
    public UserId UserId { get; private set; }
    public string Email { get; private set; } = null!;
    public bool EmailConfirmed { get; set; } = false;
    public string Name { get; private set; } = null!;
    public Language Language { get; private set; } = Language.English;
    public IEnumerable<string> SessionIds { get; set; } = new List<string>();

    public void AddSessionId(string sessionId)
    {
        SessionIds = SessionIds.Append(sessionId);
    }

    public async Task Load(ICache cache, string userId)
    {
        var sessionKey = CacheKeys.UserSession(userId);
        var session = await cache.GetAsync<Session>(sessionKey);
        if (session is null) return;

        UserId = session.UserId;
        Email = session.Email;
        EmailConfirmed = session.EmailConfirmed;
        Name = session.Name;
        Language = session.Language;
        SessionIds = session.SessionIds;
    }
}