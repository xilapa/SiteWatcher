namespace SiteWatcher.WebAPI.DTOs.ViewModels;

public class AuthenticationResult
{
    public void Set(EAuthTask task, string token)
    {
        Task = task;
        Token = token;
    }

    public EAuthTask Task { get; set; }
    public string Token { get; set; }
}

public enum EAuthTask
{
    Register = 1,
    Login = 2
}