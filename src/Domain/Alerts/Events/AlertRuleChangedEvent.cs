using SiteWatcher.Domain.Common.Events;
using SiteWatcher.Domain.Common.ValueObjects;

namespace SiteWatcher.Domain.Alerts.Events;

public sealed class AlertRuleChangedEvent : BaseEvent
{
    public AlertRuleChangedEvent(RuleId oldRuleId)
    {
        OldRuleId = oldRuleId;
    }

    public RuleId OldRuleId { get; }
}