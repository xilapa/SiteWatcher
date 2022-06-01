namespace SiteWatcher.Application.Users.Commands.UpdateUser;

public class UpdateUserResult
{
    public UpdateUserResult(string token, bool confirmationEmailSend)
    {
        Token = token;
        ConfirmationEmailSend = confirmationEmailSend;
    }

    public string Token { get; }
    public bool ConfirmationEmailSend { get; }
}