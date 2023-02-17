using SiteWatcher.Domain.Common.ValueObjects;
using SiteWatcher.Domain.Users.DTOs;
using SiteWatcher.Domain.Users.Enums;

namespace SiteWatcher.Domain.Authentication;

public sealed class AuthSession
{
    public AuthSession()
    {
        
    }

    public AuthSession(AuthTask task, UserViewModel user, string? profilePicUrl = null)
    {
        Task = task;
        UserId = user.Id;
        GoogleId = string.Empty;
        Name = user.Name;
        Email = user.Email;
        EmailConfirmed = user.EmailConfirmed;
        Language = user.Language;
        Theme = user.Theme;
        ProfilePicUrl = profilePicUrl;
    }

    public AuthSession(AuthTask task, string googleId, string name, string email, Language lang, string? profilePicUrl = null)
    {
        Task = task;
        GoogleId = googleId;
        Name = name;
        Email = email;
        EmailConfirmed = false;
        Language = lang;
        ProfilePicUrl = profilePicUrl;
    }

    // If AuthTask is Register or Error, some informations can be null
    public AuthTask Task { get; set; }
    public UserId? UserId { get; set; }
    public string? GoogleId { get; set; }
    public string? Name { get; set; }
    public string? Email { get; set; }
    public bool EmailConfirmed { get; set; }
    public Language? Language { get; set; }
    public Theme? Theme { get; set; }
    public string? ProfilePicUrl { get; set; }
    public string? RegisterToken { get; set; }
    public string? ActivateToken { get; set; }
}