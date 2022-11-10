using System.Text.Json;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SiteWatcher.Domain.Models.Emails;

namespace SiteWatcher.Infra.Persistence.Configuration;

public class MailRecipientsValueConverter : ValueConverter<List<MailRecipient>, string>
{
    public MailRecipientsValueConverter() : base(
        recipients => JsonSerializer.Serialize(recipients, (JsonSerializerOptions)null!),
        json => JsonSerializer.Deserialize<List<MailRecipient>>(json, (JsonSerializerOptions)null!)!)
    {
    }
}