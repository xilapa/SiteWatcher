using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using SiteWatcher.Domain.Models.Common;

namespace SiteWatcher.Infra.Configuration;

internal class AlertIdGenerator : ValueGenerator<AlertId>
{
    public override bool GeneratesTemporaryValues => true;

    public override AlertId Next(EntityEntry entry) => new (-1);
}
