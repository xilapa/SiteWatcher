﻿using FluentValidation;
using SiteWatcher.Application.Common.Constants;
using SiteWatcher.Application.Common.Validation;
using SiteWatcher.Domain.Alerts.Enums;

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
            .NotEqual(default(Frequencies))
            .WithMessage(ApplicationErrors.ValueIsInvalid(nameof(CreateAlertCommand.Frequency)))
            .Must(f => Enum.IsDefined(typeof(Frequencies), (int) f))
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

        RuleFor(cmmd => cmmd.Rule)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage(ApplicationErrors.ValueIsInvalid(nameof(CreateAlertCommand.Rule)))
            .NotEqual(default(RuleType))
            .WithMessage(ApplicationErrors.ValueIsInvalid(nameof(CreateAlertCommand.Rule)))
            .Must(wm => Enum.IsDefined(typeof(RuleType), (int) wm))
            .WithMessage(ApplicationErrors.ValueIsInvalid(nameof(CreateAlertCommand.Rule)));

        // Term watch validation
        RuleFor<string>(cmmd => cmmd.Term)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage(ApplicationErrors.ValueIsNullOrEmpty(nameof(CreateAlertCommand.Term)))
            .MinimumLength(3)
            .WithMessage(ApplicationErrors.ValueBellowMinimumLength(nameof(CreateAlertCommand.Term)))
            .MaximumLength(64)
            .WithMessage(ApplicationErrors.ValueAboveMaximumLength(nameof(CreateAlertCommand.Term)))
            .When(cmmd => RuleType.Term.Equals(cmmd.Rule));

        // Regex rule validation
        RuleFor<string>(cmmd => cmmd.RegexPattern)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage(ApplicationErrors.ValueIsNullOrEmpty(nameof(CreateAlertCommand.RegexPattern)))
            .MaximumLength(512)
            .WithMessage(ApplicationErrors.ValueAboveMaximumLength(nameof(CreateAlertCommand.RegexPattern)))
            .IsValidRegex()
            .WithMessage(ApplicationErrors.ValueIsInvalid(nameof(CreateAlertCommand.RegexPattern)))
            .When(cmmd => RuleType.Regex.Equals(cmmd.Rule));

        RuleFor(cmmd => cmmd.NotifyOnDisappearance)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage(ApplicationErrors.ValueIsNullOrEmpty(nameof(CreateAlertCommand.NotifyOnDisappearance)))
            .When(cmmd => RuleType.Regex.Equals(cmmd.Rule));
    }
}