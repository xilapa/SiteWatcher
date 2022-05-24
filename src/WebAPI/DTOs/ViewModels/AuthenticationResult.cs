namespace SiteWatcher.WebAPI.DTOs.ViewModels;

public class AuthenticationResult
{
    public void Set(EAuthTask task, string token, string? profilePicUrl)
    {
        Task = task;
        Token = token;
        ProfilePicUrl = profilePicUrl;
    }

    public EAuthTask Task { get; set; }
    public string Token { get; set; }
    public string? ProfilePicUrl { get; set; }
}

public enum EAuthTask
{
    Register = 1,
    Login = 2
}