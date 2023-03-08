namespace Domain.Authentication;

public sealed class AuthKeys
{
    public AuthKeys(string key, string secutriyToken)
    {
        Key = key;
        SecutriyToken = secutriyToken;
    }

    public AuthKeys(string errorMessage)
    {
        ErrorMessage = errorMessage;
    }

    public string Key { get; set; } = null!;
    public string SecutriyToken { get; set; } = null!;
    public string? ErrorMessage { get; set; }

    public bool Success() => string.IsNullOrEmpty(ErrorMessage);
}