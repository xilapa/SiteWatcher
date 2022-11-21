using System.Text.Json;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace SiteWatcher.Infra.Persistence.Configuration;

public sealed class RegexWatchMatchesValueConverter : ValueConverter<List<string>, string>
{
    public RegexWatchMatchesValueConverter() : base(
        matches => JsonSerializer.Serialize(matches, (JsonSerializerOptions)null!),
        json => JsonSerializer.Deserialize<List<string>>(json, (JsonSerializerOptions)null!)!)
    { }
}