using FluentValidation;
using SiteWatcher.Application.Common.Constants;
using SiteWatcher.Application.Common.Validation;
using SiteWatcher.Domain.Enums;

namespace SiteWatcher.Application.Users.Commands.UpdateUser;

public class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator()
    {
        RuleFor(cmmd => cmmd.Name)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage(ApplicationErrors.ValueIsNullOrEmpty(nameof(UpdateUserCommand.Name)))
            .MinimumLength(3)
            .WithMessage(ApplicationErrors.ValueBellowMinimumLength(nameof(UpdateUserCommand.Name)))
            .HasOnlyLetters()
            .WithMessage(ApplicationErrors.NAME_MUST_HAVE_ONLY_LETTERS);

        RuleFor(cmmd => cmmd.Email)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage(ApplicationErrors.ValueIsNullOrEmpty(nameof(UpdateUserCommand.Email)))
            .EmailAddress()
            .WithMessage(ApplicationErrors.ValueIsInvalid(nameof(UpdateUserCommand.Email)));

        RuleFor(cmmd => cmmd.Language)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage(ApplicationErrors.ValueIsInvalid(nameof(UpdateUserCommand.Language)))
            .NotEqual(default(ELanguage))
            .WithMessage(ApplicationErrors.ValueIsInvalid(nameof(UpdateUserCommand.Language)))
            .Must(l => Enum.IsDefined(typeof(ELanguage), (int) l))
            .WithMessage(ApplicationErrors.ValueIsInvalid(nameof(UpdateUserCommand.Language)));

        RuleFor(cmmd => cmmd.Theme)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage(ApplicationErrors.ValueIsInvalid(nameof(UpdateUserCommand.Theme)))
            .NotEqual(default(ETheme))
            .WithMessage(ApplicationErrors.ValueIsInvalid(nameof(UpdateUserCommand.Theme)))
            .Must(t => Enum.IsDefined(typeof(ETheme), (int) t))
            .WithMessage(ApplicationErrors.ValueIsInvalid(nameof(UpdateUserCommand.Theme)));
    }
}