using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SiteWatcher.Domain.Alerts.Enums;

namespace SiteWatcher.Infra.Persistence.Configuration;

public class RuleConverter : ValueConverter<RuleType,char>
{
    public RuleConverter() : base(v => (char)v, v => (RuleType)v)
    { }
}