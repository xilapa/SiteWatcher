namespace SiteWatcher.Domain.Authentication;

public sealed class AuthenticationResult
{
    public AuthenticationResult(AuthTask task, string token, string? profilePicUrl)
    {
        Task = task;
        Token = token;
        ProfilePicUrl = profilePicUrl;
    }

    public AuthTask Task { get; set; }
    public string Token { get; set; }
    public string? ProfilePicUrl { get; set; }
}

public enum AuthTask
{
    Register = 1,
    Login,
    Activate
}