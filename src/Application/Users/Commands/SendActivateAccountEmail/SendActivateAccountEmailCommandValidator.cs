using FluentValidation;
using SiteWatcher.Application.Common.Constants;

namespace SiteWatcher.Application.Users.Commands.ActivateAccount;

public class SendActivateAccountEmailCommandValidator : AbstractValidator<SendActivateAccountEmailCommand>
{
    public SendActivateAccountEmailCommandValidator()
    {
        RuleFor(cmmd => cmmd.UserId)
            .NotEmpty()
            .WithMessage(ApplicationErrors.USER_DO_NOT_EXIST);
    }
}