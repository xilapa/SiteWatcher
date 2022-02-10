using SiteWatcher.Application.DTOS.InputModels;
using SiteWatcher.Application.Constants;
using FluentValidation;

namespace SiteWatcher.Application.Validators;

public class UserSubscribeValidator : AbstractValidator<UserSubscribeIM>
{
    public UserSubscribeValidator()
    {
        RuleFor(u => u.Name)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
                .WithMessage(ApplicationErrors.USERNAME_NOT_BE_NULL_OR_EMPTY)
            .MinimumLength(3)
                .WithMessage(ApplicationErrors.USERNAME_MINIMUM_LENGTH);

        RuleFor(u => u.Email)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
                .WithMessage(ApplicationErrors.EMAIL_NOT_BE_NULL_OR_EMPTY)
            .EmailAddress()
                .WithMessage(ApplicationErrors.EMAIL_IS_INVALID);
    }
}