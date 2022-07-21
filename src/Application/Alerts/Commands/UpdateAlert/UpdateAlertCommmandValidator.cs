﻿using FluentValidation;
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

        RuleFor(cmmd => cmmd.AlertId)
            .NotEmpty()
            .WithMessage(ApplicationErrors.ValueIsNullOrEmpty(nameof(UpdateAlertCommmand.AlertId)));

        RuleFor(cmmd => cmmd.Name!.NewValue)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage(ApplicationErrors.ValueIsNullOrEmpty(nameof(UpdateAlertCommmand.Name)))
            .MinimumLength(3)
            .WithMessage(ApplicationErrors.ValueBellowMinimumLength(nameof(UpdateAlertCommmand.Name)))
            .MaximumLength(64)
            .WithMessage(ApplicationErrors.ValueAboveMaximumLength(nameof(UpdateAlertCommmand.Name)))
            .When(cmmd => cmmd.Name is not null);

        RuleFor(cmmd => cmmd.Frequency!.NewValue)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage(ApplicationErrors.ValueIsInvalid(nameof(UpdateAlertCommmand.Frequency)))
            .NotEqual(default(EFrequency))
            .WithMessage(ApplicationErrors.ValueIsInvalid(nameof(UpdateAlertCommmand.Frequency)))
            .When(cmmd => cmmd.Frequency is not null);

        RuleFor(cmmd => cmmd.SiteName!.NewValue)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage(ApplicationErrors.ValueIsNullOrEmpty(nameof(UpdateAlertCommmand.SiteName)))
            .MinimumLength(3)
            .WithMessage(ApplicationErrors.ValueBellowMinimumLength(nameof(UpdateAlertCommmand.SiteName)))
            .MaximumLength(64)
            .WithMessage(ApplicationErrors.ValueAboveMaximumLength(nameof(UpdateAlertCommmand.SiteName)))
            .When(cmmd => cmmd.SiteName is not null);

        RuleFor(cmmd => cmmd.SiteUri!.NewValue)
            .Must(uri => Uri.IsWellFormedUriString(uri, UriKind.Absolute))
            .WithMessage(ApplicationErrors.ValueIsInvalid(nameof(UpdateAlertCommmand.SiteUri)))
            .When(cmmd => cmmd.SiteUri is not null);

        RuleFor(cmmd => cmmd.WatchMode!.NewValue)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage(ApplicationErrors.ValueIsInvalid(nameof(UpdateAlertCommmand.WatchMode)))
            .NotEqual(default(EWatchMode))
            .WithMessage(ApplicationErrors.ValueIsInvalid(nameof(UpdateAlertCommmand.WatchMode)))
            .When(cmmd => cmmd.WatchMode is not null);

        // Term watch validation
        RuleFor(cmmd => cmmd.Term)
            .NotEmpty()
            .WithMessage(ApplicationErrors.ValueIsNullOrEmpty(nameof(UpdateAlertCommmand.Term)))
            .When(cmmd => cmmd.WatchMode is not null && EWatchMode.Term.Equals(cmmd.WatchMode.NewValue));

        RuleFor(cmmd => cmmd.Term!.NewValue)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage(ApplicationErrors.ValueIsNullOrEmpty(nameof(UpdateAlertCommmand.Term)))
            .MinimumLength(3)
            .WithMessage(ApplicationErrors.ValueBellowMinimumLength(nameof(UpdateAlertCommmand.Term)))
            .MaximumLength(64)
            .WithMessage(ApplicationErrors.ValueAboveMaximumLength(nameof(UpdateAlertCommmand.Term)))
            .When(cmmd => cmmd.WatchMode is not null
                          && EWatchMode.Term.Equals(cmmd.WatchMode.NewValue)
                          && cmmd.Term is not null);
    }
}