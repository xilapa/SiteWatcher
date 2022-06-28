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
            .WithMessage(ApplicationErrors.ValueIsNullOrEmpty(nameof(RegisterUserCommand.Name)))
            .MinimumLength(3)
            .WithMessage(ApplicationErrors.ValueBellowMinimumLength(nameof(RegisterUserCommand.Name)))
            .HasOnlyLetters()
            .WithMessage(ApplicationErrors.NAME_MUST_HAVE_ONLY_LETTERS);

        RuleFor(cmmd => cmmd.Email)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage(ApplicationErrors.ValueIsNullOrEmpty(nameof(RegisterUserCommand.Email)))
            .EmailAddress()
            .WithMessage(ApplicationErrors.ValueIsInvalid(nameof(RegisterUserCommand.Email)));

        RuleFor(cmmd => cmmd.Language)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage(ApplicationErrors.ValueIsInvalid(nameof(RegisterUserCommand.Language)))
            .NotEqual(default(ELanguage))
            .WithMessage(ApplicationErrors.ValueIsInvalid(nameof(RegisterUserCommand.Language)));

        RuleFor(cmmd => cmmd.Theme)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage(ApplicationErrors.ValueIsInvalid(nameof(RegisterUserCommand.Theme)))
            .NotEqual(default(ETheme))
            .WithMessage(ApplicationErrors.ValueIsInvalid(nameof(RegisterUserCommand.Theme)));
    }
}