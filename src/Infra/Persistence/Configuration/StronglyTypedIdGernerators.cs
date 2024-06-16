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
internal sealed class AlertIdGenerator : StrongIdIntGenerator<AlertId>
{
    public override AlertId Next(EntityEntry entry) => new(-1);
}

public sealed class AlertIdValueGeneratorFactory : ValueGeneratorFactory
{
    public override ValueGenerator Create(IProperty property, ITypeBase typeBase)
    {
        return new AlertIdGenerator();
    }
}
#endregion

#region RuleId
internal sealed class RuleIdGenerator : StrongIdIntGenerator<RuleId>
{
    public override RuleId Next(EntityEntry entry) => new(-1);
}

public sealed class RuleIdValueGeneratorFactory : ValueGeneratorFactory
{
    public override ValueGenerator Create(IProperty property, ITypeBase typeBase)
    {
        return new RuleIdGenerator();
    }
}
#endregion

#region NotificationId
internal sealed class NotificationIdGenerator : StrongIdIntGenerator<NotificationId>
{
    public override NotificationId Next(EntityEntry entry) => new ();
}
#endregion