using SiteWatcher.Domain.Common.ValueObjects;
using SiteWatcher.Domain.Users.Enums;

namespace SiteWatcher.Domain.Authentication;

public sealed class SessionView
{
    public SessionView(AuthSession authSession, string sessionId)
    {
        Task = authSession.Task;
        Name = authSession.Name;
        Email = authSession.Email;
        EmailConfirmed = authSession.EmailConfirmed;
        Language = authSession.Language;
        ProfilePicUrl = authSession.ProfilePicUrl;
        SessionId = sessionId;
        ProfilePicUrl = authSession.ProfilePicUrl;
    }

    public SessionView(Session session, string sessionId, string? profilePicUrl = null)
    {
        Task = AuthTask.Login;
        Name = session.Name;
        Email = session.Email;
        EmailConfirmed = session.EmailConfirmed;
        Language = session.Language;
        SessionId = sessionId;
        ProfilePicUrl = profilePicUrl;
    }

    public SessionView(AuthTask task)
    {
        Task = task;
    }

    public AuthTask Task { get; set; }
    public UserId? UserId { get; set; }
    public string SessionId { get; set; }
    public string? Name { get; set; }
    public string? Email { get; set; }
    public bool EmailConfirmed { get; set; }
    public Language? Language { get; set; }
    public string? ProfilePicUrl { get; set; }
}