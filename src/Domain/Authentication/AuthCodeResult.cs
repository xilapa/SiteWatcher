namespace Domain.Authentication;

public sealed class AuthCodeResult
{
    public AuthCodeResult(string? code, string? errorMessage = null!)
    {
        Code = code;
        ErrorMessage = errorMessage ?? string.Empty;
    }

    public string? Code { get; set; }
    public string ErrorMessage { get; set; }

    public bool Success() => string.IsNullOrEmpty(ErrorMessage);
}