namespace SiteWatcher.Application.Authentication.Common;

public class AuthenticationResult
{
    public AuthenticationResult(AuthTask task, string token, string? profilePicUrl = null, string message = null)
    {
        Task = task;
        Token = token;
        ProfilePicUrl = profilePicUrl ?? string.Empty;
        Message = message ?? string.Empty;
    }

    public AuthTask Task { get; set; }
    public string Token { get; set; }
    public string? ProfilePicUrl { get; set; }
    public string Message { get; set; }
}

public enum AuthTask
{
    Register = 1,
    Login,
    Activate,
    Error
}