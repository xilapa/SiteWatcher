using FluentValidation;
using SiteWatcher.Application.Alerts.Commands.CreateAlert;
using SiteWatcher.Application.Common.Constants;
using SiteWatcher.Application.Common.Validation;
using SiteWatcher.Domain.Alerts.Enums;

namespace SiteWatcher.Application.Alerts.Commands.UpdateAlert;

// TODO: simplify validation removing FluentValidation
public class UpdateAlertCommmandValidator : AbstractValidator<UpdateAlertCommmand>
{
    public UpdateAlertCommmandValidator()
    {
        RuleFor(cmmd => cmmd)
            .Must(cmmd => !(cmmd.Name is null &&
                            cmmd.Frequency is null &&
                            cmmd.SiteName is null &&
                            cmmd.SiteUri is null &&
                            cmmd.Rule is null &&
                            cmmd.Term is null &&
                            cmmd.RegexPattern is null &&
                            cmmd.NotifyOnDisappearance is null))
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
            .NotEqual(default(Frequencies))
            .WithMessage(ApplicationErrors.ValueIsInvalid(nameof(UpdateAlertCommmand.Frequency)))
            .Must(f => Enum.IsDefined(typeof(Frequencies), (int) f))
            .WithMessage(ApplicationErrors.ValueIsInvalid(nameof(CreateAlertCommand.Frequency)))
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

        RuleFor(cmmd => cmmd.Rule!.NewValue)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage(ApplicationErrors.ValueIsInvalid(nameof(UpdateAlertCommmand.Rule)))
            .NotEqual(default(Rules))
            .WithMessage(ApplicationErrors.ValueIsInvalid(nameof(UpdateAlertCommmand.Rule)))
            .Must(wm => Enum.IsDefined(typeof(Rules), (int) wm))
            .WithMessage(ApplicationErrors.ValueIsInvalid(nameof(CreateAlertCommand.Rule)))
            .When(cmmd => cmmd.Rule is not null);

        // Term rule validation
        RuleFor(cmmd => cmmd.Term)
            .NotEmpty()
            .WithMessage(ApplicationErrors.ValueIsNullOrEmpty(nameof(UpdateAlertCommmand.Term)))
            .When(cmmd => cmmd.Rule is not null
                        && Rules.Term.Equals(cmmd.Rule.NewValue));

        RuleFor(cmmd => cmmd.Term!.NewValue)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage(ApplicationErrors.ValueIsNullOrEmpty(nameof(UpdateAlertCommmand.Term)))
            .MinimumLength(3)
            .WithMessage(ApplicationErrors.ValueBellowMinimumLength(nameof(UpdateAlertCommmand.Term)))
            .MaximumLength(64)
            .WithMessage(ApplicationErrors.ValueAboveMaximumLength(nameof(UpdateAlertCommmand.Term)))
            .When(cmmd => cmmd.Rule is not null
                          && Rules.Term.Equals(cmmd.Rule.NewValue)
                          && cmmd.Term is not null);

        // Regex rule validation
        // TODO: validate if all regex rule fields are sent, to avoid null exception
        RuleFor(cmmd => cmmd.RegexPattern)
            .NotEmpty()
            .WithMessage(ApplicationErrors.ValueIsNullOrEmpty(nameof(UpdateAlertCommmand.RegexPattern)))
            .When(cmmd => cmmd.Rule is not null
                            && Rules.Regex.Equals(cmmd.Rule.NewValue));

        RuleFor(cmmd => cmmd.RegexPattern!.NewValue)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage(ApplicationErrors.ValueIsNullOrEmpty(nameof(CreateAlertCommand.RegexPattern)))
            .MaximumLength(512)
            .WithMessage(ApplicationErrors.ValueAboveMaximumLength(nameof(CreateAlertCommand.RegexPattern)))
            .IsValidRegex()
            .WithMessage(ApplicationErrors.ValueIsInvalid(nameof(CreateAlertCommand.RegexPattern)))
            .When(cmmd => cmmd.RegexPattern?.NewValue != null);

        RuleFor(cmmd => cmmd.NotifyOnDisappearance)
            .Must(updateNotifyOnDis => updateNotifyOnDis != null)
            .WithMessage(ApplicationErrors.ValueIsNullOrEmpty(nameof(UpdateAlertCommmand.NotifyOnDisappearance)))
            .When(cmmd => cmmd.Rule is not null
                            && Rules.Regex.Equals(cmmd.Rule.NewValue));

        RuleFor(cmmd => cmmd.NotifyOnDisappearance!.NewValue)
            .Must(updateNotifyOnDis => updateNotifyOnDis != null)
            .WithMessage(ApplicationErrors.ValueIsNullOrEmpty(nameof(UpdateAlertCommmand.NotifyOnDisappearance)))
            .When(cmmd => cmmd.Rule is not null
                            && Rules.Regex.Equals(cmmd.Rule.NewValue));
    }
}