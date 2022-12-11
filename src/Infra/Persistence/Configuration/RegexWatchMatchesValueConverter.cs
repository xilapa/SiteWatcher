using System.Text.Json;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SiteWatcher.Domain.Alerts.Entities.WatchModes;

namespace SiteWatcher.Infra.Persistence.Configuration;

public sealed class RegexWatchMatchesValueConverter : ValueConverter<Matches, string>
{
    public RegexWatchMatchesValueConverter() : base(
        matches => JsonSerializer.Serialize(matches, (JsonSerializerOptions)null!),
        json => JsonSerializer.Deserialize<Matches>(json, (JsonSerializerOptions)null!)!)
    { }
}