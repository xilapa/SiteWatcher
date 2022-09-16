namespace SiteWatcher.Application.Users.Commands.RegisterUser;

public abstract class RegisterUserResult
{
    private static AlreadyExists _alreadyExists => new();
    public static AlreadyExists AlreadyExists() => _alreadyExists;
}

public sealed class Registered : RegisterUserResult
{
    public Registered(string token, bool confirmationEmailSend)
    {
        Token = token;
        ConfirmationEmailSend = confirmationEmailSend;
    }

    public string Token { get; }
    public bool ConfirmationEmailSend { get; }
}

public sealed class AlreadyExists : RegisterUserResult
{ }