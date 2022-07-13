using FluentValidation;
using SiteWatcher.Application.Common.Constants;

namespace SiteWatcher.Application.Alerts.Commands.DeleteAlert;

public class DeleteAlertCommandValidator : AbstractValidator<DeleteAlertCommand>
{
    public DeleteAlertCommandValidator()
    {
        RuleFor(cmmd => cmmd.AlertId)
            .NotEmpty()
            .WithMessage(ApplicationErrors.ValueIsNullOrEmpty(nameof(DeleteAlertCommand.AlertId)));
    }
}