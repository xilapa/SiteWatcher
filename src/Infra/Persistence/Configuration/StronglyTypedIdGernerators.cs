using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
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

public class AlertIdValueGeneratorFactory : ValueGeneratorFactory
{
    public override ValueGenerator Create(IProperty property, IEntityType entityType)
    {
        return new AlertIdGenerator();
    }
}

internal class WatchModeIdGenerator : StrongIdIntGenerator<WatchModeId>
{
    public override WatchModeId Next(EntityEntry entry) => new (-1);
}

public class WatchModeIdValueGeneratorFactory : ValueGeneratorFactory
{
    public override ValueGenerator Create(IProperty property, IEntityType entityType)
    {
        return new WatchModeIdGenerator();
    }
}
