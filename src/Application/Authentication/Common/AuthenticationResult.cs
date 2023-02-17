namespace SiteWatcher.Application.Authentication.Common;

public class AuthenticationResult
{
    public AuthenticationResult(string token, string? error = null)
    {
        Token = token;
        Error = error ?? string.Empty;
        Success = string.IsNullOrEmpty(error);
    }

    public string Token { get; set; }
    public bool Success { get; }
    public string Error { get; set; }
}