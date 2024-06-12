using SiteWatcher.Common.Services;
using SiteWatcher.Domain.Alerts.DTOs;
using SiteWatcher.Domain.Alerts.Enums;
using SiteWatcher.Domain.Common;

namespace Application.Alerts.Dtos;

public sealed class SimpleAlertViewDto
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public DateTime CreatedAt { get; set; }
    public Frequencies Frequency { get; set; }
    public DateTime? LastVerification { get; set; }
    public int TriggeringsCount { get; set; }
    public string? SiteName { get; set; }
    public char RuleType { get; set; }

    public SimpleAlertView ToSimpleAlertView(IIdHasher idHasher) =>
        new ()
        {
            Id = idHasher.HashId(Id),
            Name = Name,
            CreatedAt = CreatedAt,
            Frequency = Frequency,
            LastVerification = LastVerification,
            TriggeringsCount = TriggeringsCount,
            SiteName = SiteName,
            Rule = (RuleType)RuleType
        };
}