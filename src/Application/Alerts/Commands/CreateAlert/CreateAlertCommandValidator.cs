using FluentValidation;
using SiteWatcher.Application.Common.Constants;
using SiteWatcher.Domain.Enums;

namespace SiteWatcher.Application.Alerts.Commands.CreateAlert;

public class CreateAlertCommandValidator : AbstractValidator<CreateAlertCommand>
{
    public CreateAlertCommandValidator()
    {
        RuleFor(cmmd => cmmd.Name)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage(ApplicationErrors.ValueIsNullOrEmpty(nameof(CreateAlertCommand.Name)))
            .MinimumLength(3)
            .WithMessage(ApplicationErrors.ValueBellowMinimumLength(nameof(CreateAlertCommand.Name)))
            .MaximumLength(64)
            .WithMessage(ApplicationErrors.ValueAboveMaximumLength(nameof(CreateAlertCommand.Name)));

        RuleFor(cmmd => cmmd.Frequency)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage(ApplicationErrors.ValueIsInvalid(nameof(CreateAlertCommand.Frequency)))
            .NotEqual(default(EFrequency))
            .WithMessage(ApplicationErrors.ValueIsInvalid(nameof(CreateAlertCommand.Frequency)));

        RuleFor(cmmd => cmmd.SiteName)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage(ApplicationErrors.ValueIsNullOrEmpty(nameof(CreateAlertCommand.SiteName)))
            .MinimumLength(3)
            .WithMessage(ApplicationErrors.ValueBellowMinimumLength(nameof(CreateAlertCommand.SiteName)))
            .MaximumLength(64)
            .WithMessage(ApplicationErrors.ValueAboveMaximumLength(nameof(CreateAlertCommand.SiteName)));

        RuleFor(cmmd => cmmd.SiteUri)
            .Must(uri => Uri.IsWellFormedUriString(uri, UriKind.Absolute))
            .WithMessage(ApplicationErrors.ValueIsInvalid(nameof(CreateAlertCommand.SiteUri)));

        RuleFor(cmmd => cmmd.WatchMode)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage(ApplicationErrors.ValueIsInvalid(nameof(CreateAlertCommand.WatchMode)))
            .NotEqual(default(EWatchMode))
            .WithMessage(ApplicationErrors.ValueIsInvalid(nameof(CreateAlertCommand.WatchMode)));

        // Term watch validation
        RuleFor(cmmd => cmmd.Term)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage(ApplicationErrors.ValueIsNullOrEmpty(nameof(CreateAlertCommand.Term)))
            .MinimumLength(3)
            .WithMessage(ApplicationErrors.ValueBellowMinimumLength(nameof(CreateAlertCommand.Term)))
            .MaximumLength(64)
            .WithMessage(ApplicationErrors.ValueAboveMaximumLength(nameof(CreateAlertCommand.Term)))
            .When(cmmd => EWatchMode.Term.Equals(cmmd.WatchMode));
    }
}