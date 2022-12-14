using System.Text.Json;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SiteWatcher.Domain.Alerts.Entities.Rules;

namespace SiteWatcher.Infra.Persistence.Configuration;

public sealed class RegexRuleMatchesValueConverter : ValueConverter<Matches, string>
{
    public RegexRuleMatchesValueConverter() : base(
        matches => JsonSerializer.Serialize(matches, (JsonSerializerOptions)null!),
        json => JsonSerializer.Deserialize<Matches>(json, (JsonSerializerOptions)null!)!)
    { }
}