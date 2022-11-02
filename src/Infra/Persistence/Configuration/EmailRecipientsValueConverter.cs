using System.Text.Json;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SiteWatcher.Domain.Models.Emails;

namespace SiteWatcher.Infra.Persistence.Configuration;

public class EmailRecipientsValueConverter : ValueConverter<List<EmailRecipient>, string>
{
    public EmailRecipientsValueConverter() : base(
        recipients => JsonSerializer.Serialize(recipients, (JsonSerializerOptions)null!),
        json => JsonSerializer.Deserialize<List<EmailRecipient>>(json, (JsonSerializerOptions)null!)!)
    {
    }
}