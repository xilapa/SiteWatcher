using FluentValidation;
using SiteWatcher.Application.Common.Constants;
using SiteWatcher.Application.Common.Validation;
using SiteWatcher.Domain.Enums;

namespace SiteWatcher.Application.Users.Commands.RegisterUser;

public class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator()
    {
        RuleFor(cmmd => cmmd.Name)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
                .WithMessage(ApplicationErrors.NAME_NOT_BE_NULL_OR_EMPTY)
            .MinimumLength(3)
                .WithMessage(ApplicationErrors.NAME_MINIMUM_LENGTH)
            .HasOnlyLetters()
                .WithMessage(ApplicationErrors.NAME_MUST_HAVE_ONLY_LETTERS);

        RuleFor(cmmd => cmmd.Email)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
                .WithMessage(ApplicationErrors.EMAIL_NOT_BE_NULL_OR_EMPTY)
            .EmailAddress()
                .WithMessage(ApplicationErrors.EMAIL_IS_INVALID);

        RuleFor(cmmd => cmmd.Language)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
                .WithMessage(ApplicationErrors.LANGUAGE_IS_INVALID)
            .NotEqual(default(ELanguage))
                .WithMessage(ApplicationErrors.LANGUAGE_IS_INVALID);
    }
}