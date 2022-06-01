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
            .Cascade(CascadeMode.Continue)
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

        RuleFor(cmmd => cmmd.Theme)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage(ApplicationErrors.THEME_IS_INVALID)
            .NotEqual(default(ETheme))
            .WithMessage(ApplicationErrors.THEME_IS_INVALID);
    }
}