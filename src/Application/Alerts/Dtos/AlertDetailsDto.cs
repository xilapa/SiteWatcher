using SiteWatcher.Common.Services;
using SiteWatcher.Domain.Alerts.DTOs;

namespace Application.Alerts.Dtos;

public sealed class AlertDetailsDto
{
    public int Id { get; set; }
    public string SiteUri { get; set; } = null!;
    public int RuleId { get; set; }
    public string? Term { get; set; }
    public string? RegexPattern { get; set; }
    public bool? NotifyOnDisappearance { get; set; }

    public AlertDetails ToAlertDetails(IIdHasher idHasher) =>
        new()
        {
            Id = idHasher.HashId(Id),
            SiteUri = SiteUri,
            // TODO: remove this Id
            RuleId = idHasher.HashId(RuleId),
            Term = Term,
            RegexPattern = RegexPattern,
            NotifyOnDisappearance = NotifyOnDisappearance
        };
}