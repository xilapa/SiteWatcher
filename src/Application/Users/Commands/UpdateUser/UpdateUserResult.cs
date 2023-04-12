using SiteWatcher.Domain.Users.DTOs;

namespace SiteWatcher.Application.Users.Commands.UpdateUser;

public class UpdateUserResult
{
    public UpdateUserResult(UserViewModel user, bool confirmationEmailSent)
    {
        User = user;
        ConfirmationEmailSent = confirmationEmailSent;
    }

    public UserViewModel User { get; }
    public bool ConfirmationEmailSent { get; }
}