using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SiteWatcher.Domain.Models.Alerts;
using SiteWatcher.Domain.Models.Alerts.WatchModes;
using SiteWatcher.Domain.Models.Common;

namespace SiteWatcher.Infra.Configuration;

public class AlertMapping : BaseModelMapping<Alert, AlertId>
{
    public override void Configure(EntityTypeBuilder<Alert> builder)
    {
        base.Configure(builder);

        builder.ToTable("Alerts");

        builder.Property(a => a.Id)
            .HasConversion<AlertId.EfCoreValueConverter>()
            .HasValueGenerator<AlertIdGenerator>()
            .HasColumnType("integer")
            .UseIdentityColumn();

        builder.Property(a => a.UserId)
            .HasConversion<UserId.EfCoreValueConverter>()
            .HasColumnType("uuid");

        builder.HasIndex(a => a.UserId)
            .IsUnique(false);

        builder.Property(a => a.Name)
            .HasColumnType("varchar(64)")
            .IsRequired();

        builder.Property(a => a.Frequency)
            .HasColumnType("smallint")
            .IsRequired();

        builder.Property(a => a.LastVerification)
            .HasColumnType("timestamp")
            .IsRequired(false);

        builder.OwnsOne(a => a.Site,
            sb =>
            {
                sb.Property(s => s.Name)
                    .HasColumnType("varchar(64)")
                    .IsRequired();

                sb.Property(s => s.Uri)
                    .HasColumnType("varchar(512)")
                    .HasConversion(
                        v => v.ToString(),
                        v => new Uri(v))
                    .IsRequired();
            });

        builder.HasOne(a => a.WatchMode)
            .WithOne()
            .HasForeignKey(nameof(WatchMode),nameof(WatchMode.AlertId));
    }
}