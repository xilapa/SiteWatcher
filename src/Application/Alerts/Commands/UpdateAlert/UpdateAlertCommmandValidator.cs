using FluentValidation;
using SiteWatcher.Application.Alerts.Commands.CreateAlert;
using SiteWatcher.Application.Common.Constants;
using SiteWatcher.Domain.Enums;

namespace SiteWatcher.Application.Alerts.Commands.UpdateAlert;

public class UpdateAlertCommmandValidator : AbstractValidator<UpdateAlertCommmand>
{
    public UpdateAlertCommmandValidator()
    {
        RuleFor(cmmd => cmmd)
            .Must(cmmd => !(cmmd.Name is null &&
                            cmmd.Frequency is null &&
                            cmmd.SiteName is null &&
                            cmmd.SiteUri is null &&
                            cmmd.WatchMode is null &&
                            cmmd.Term is null))
            .WithMessage(ApplicationErrors.UPDATE_DATA_IS_NULL);

        RuleFor(cmmd => cmmd.Name!.NewValue)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage(ApplicationErrors.ValueIsNullOrEmpty(nameof(CreateAlertCommand.Name)))
            .MinimumLength(3)
            .WithMessage(ApplicationErrors.ValueBellowMinimumLength(nameof(CreateAlertCommand.Name)))
            .MaximumLength(64)
            .WithMessage(ApplicationErrors.ValueAboveMaximumLength(nameof(CreateAlertCommand.Name)))
            .When(cmmd => cmmd.Name is not null);

        RuleFor(cmmd => cmmd.Frequency!.NewValue)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage(ApplicationErrors.ValueIsInvalid(nameof(CreateAlertCommand.Frequency)))
            .NotEqual(default(EFrequency))
            .WithMessage(ApplicationErrors.ValueIsInvalid(nameof(CreateAlertCommand.Frequency)))
            .When(cmmd => cmmd.Frequency is not null);

        RuleFor(cmmd => cmmd.SiteName!.NewValue)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage(ApplicationErrors.ValueIsNullOrEmpty(nameof(CreateAlertCommand.SiteName)))
            .MinimumLength(3)
            .WithMessage(ApplicationErrors.ValueBellowMinimumLength(nameof(CreateAlertCommand.SiteName)))
            .MaximumLength(64)
            .WithMessage(ApplicationErrors.ValueAboveMaximumLength(nameof(CreateAlertCommand.SiteName)))
            .When(cmmd => cmmd.SiteName is not null);

        RuleFor(cmmd => cmmd.SiteUri!.NewValue)
            .Must(uri => Uri.IsWellFormedUriString(uri, UriKind.Absolute))
            .WithMessage(ApplicationErrors.ValueIsInvalid(nameof(CreateAlertCommand.SiteUri)))
            .When(cmmd => cmmd.SiteUri is not null);

        RuleFor(cmmd => cmmd.WatchMode!.NewValue)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage(ApplicationErrors.ValueIsInvalid(nameof(CreateAlertCommand.WatchMode)))
            .NotEqual(default(EWatchMode))
            .WithMessage(ApplicationErrors.ValueIsInvalid(nameof(CreateAlertCommand.WatchMode)))
            .When(cmmd => cmmd.WatchMode is not null);

        // Term watch validation
        RuleFor(cmmd => cmmd.Term!.NewValue)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage(ApplicationErrors.ValueIsNullOrEmpty(nameof(CreateAlertCommand.Term)))
            .MinimumLength(3)
            .WithMessage(ApplicationErrors.ValueBellowMinimumLength(nameof(CreateAlertCommand.Term)))
            .MaximumLength(64)
            .WithMessage(ApplicationErrors.ValueAboveMaximumLength(nameof(CreateAlertCommand.Term)))
            .When(cmmd => cmmd.Term is not null);
    }
}