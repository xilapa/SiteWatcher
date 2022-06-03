using FluentValidation;
using SiteWatcher.Application.Common.Constants;

namespace SiteWatcher.Application.Users.Commands.ActivateAccount;

public class SendReactivateAccountEmailCommandValidator : AbstractValidator<SendReactivateAccountEmailCommand>
{
    public SendReactivateAccountEmailCommandValidator()
    {
        RuleFor(cmmd => cmmd.UserId)
            .NotEmpty()
            .WithMessage(ApplicationErrors.USER_DO_NOT_EXIST);
    }
}