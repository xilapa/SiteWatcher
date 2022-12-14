using SiteWatcher.Domain.Alerts.Entities.Rules;
using SiteWatcher.Domain.Alerts.Enums;
using SiteWatcher.Domain.Common;

namespace SiteWatcher.Domain.Alerts.DTOs;

//TODO: move to detailed alert view
public sealed class DetailedRuleView
{
    public Rules? Rule { get; set; }
    public string? Term { get; set; }
    public string? RegexPattern { get; set; }
    public bool? NotifyOnDisappearance { get; set; }

    // TODO: remove this operator
    public static implicit operator DetailedRuleView(Rule rule)
    {
        var ruleType = Utils.GetRuleEnumByType(rule);
        return new DetailedRuleView
        {
            Rule = ruleType,
            Term = ruleType == Rules.Term ? (rule as TermRule)!.Term : null,
            RegexPattern = ruleType == Rules.Regex ? (rule as RegexRule)!.RegexPattern : null,
            NotifyOnDisappearance = ruleType == Rules.Regex ? (rule as RegexRule)!.NotifyOnDisappearance : null
        };
    }
}