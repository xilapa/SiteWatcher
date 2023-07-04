using FluentValidation;
using SiteWatcher.Application.Common.Constants;

namespace SiteWatcher.Application.Alerts.Commands.SearchAlerts;

public class SearchAlertCommandValidator : AbstractValidator<SearchAlertQuery>
{
    public SearchAlertCommandValidator()
    {
        RuleFor(cmmd => cmmd.Term)
            .NotEmpty()
            .WithMessage(ApplicationErrors.ValueIsNullOrEmpty(nameof(SearchAlertQuery.Term)));
    }
}