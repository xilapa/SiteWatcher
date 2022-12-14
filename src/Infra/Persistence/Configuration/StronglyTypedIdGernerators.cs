using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using SiteWatcher.Domain.Common.ValueObjects;

namespace SiteWatcher.Infra.Persistence.Configuration;

internal abstract class StrongIdIntGenerator<T> : ValueGenerator<T> where T : struct
{
    public override bool GeneratesTemporaryValues => true;
}

#region AlertId
internal class AlertIdGenerator : StrongIdIntGenerator<AlertId>
{
    public override AlertId Next(EntityEntry entry) => new(-1);
}

public class AlertIdValueGeneratorFactory : ValueGeneratorFactory
{
    public override ValueGenerator Create(IProperty property, IEntityType entityType)
    {
        return new AlertIdGenerator();
    }
}
#endregion

#region RuleId
internal class RuleIdGenerator : StrongIdIntGenerator<RuleId>
{
    public override RuleId Next(EntityEntry entry) => new(-1);
}

public class RuleIdValueGeneratorFactory : ValueGeneratorFactory
{
    public override ValueGenerator Create(IProperty property, IEntityType entityType)
    {
        return new RuleIdGenerator();
    }
}
#endregion

#region NotificationId
internal class NotificationIdGenerator : StrongIdIntGenerator<NotificationId>
{
    public override NotificationId Next(EntityEntry entry) => new ();
}

public class NotificationIdValueGeneratorFactory : ValueGeneratorFactory
{
    public override ValueGenerator Create(IProperty property, IEntityType entityType)
    {
        return new NotificationIdGenerator();
    }
}
#endregion