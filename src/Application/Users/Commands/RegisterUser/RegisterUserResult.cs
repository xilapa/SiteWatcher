namespace SiteWatcher.Application.Users.Commands.RegisterUser;

public class RegisterUserResult
{
    public RegisterUserResult(string token, bool confirmationEmailSend)
    {
        Token = token;
        ConfirmationEmailSend = confirmationEmailSend;
    }

    public string Token { get; }
    public bool ConfirmationEmailSend { get; }
}