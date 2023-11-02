using SiteWatcher.Domain.Alerts.Entities.Rules;
using SiteWatcher.Domain.Alerts.Enums;

namespace SiteWatcher.Domain.Alerts.DTOs;

//TODO: move to detailed alert view
public sealed class DetailedRuleView
{
    public RuleType? Rule { get; set; }
    public string? Term { get; set; }
    public string? RegexPattern { get; set; }
    public bool? NotifyOnDisappearance { get; set; }

    // TODO: remove this operator
    public static implicit operator DetailedRuleView(Rule rule)
    {
        return new DetailedRuleView
        {
            Rule = rule.RuleType,
            Term = rule.RuleType == RuleType.Term ? (rule as TermRule)!.Term : null,
            RegexPattern = rule.RuleType == RuleType.Regex ? (rule as RegexRule)!.RegexPattern : null,
            NotifyOnDisappearance = rule.RuleType == RuleType.Regex ? (rule as RegexRule)!.NotifyOnDisappearance : null
        };
    }
}