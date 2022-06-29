using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using SiteWatcher.Domain.Models.Common;

namespace SiteWatcher.Infra.Configuration;

internal abstract class StrongIdIntGenerator<T> : ValueGenerator<T> where T : struct
{
    public override bool GeneratesTemporaryValues => true;
}

internal class AlertIdGenerator : StrongIdIntGenerator<AlertId>
{
    public override AlertId Next(EntityEntry entry) => new (-1);
}

internal class WatchModeIdGenerator : StrongIdIntGenerator<WatchModeId>
{
    public override WatchModeId Next(EntityEntry entry) => new (-1);
}
