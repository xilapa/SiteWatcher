using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SiteWatcher.Domain.Models.Alerts.WatchModes;
using SiteWatcher.Domain.Models.Common;

namespace SiteWatcher.Infra.Configuration;

public class WatchModeMapping : BaseModelMapping<WatchMode, WatchModeId>
{
    public override void Configure(EntityTypeBuilder<WatchMode> builder)
    {
        base.Configure(builder);

        builder.ToTable("WatchModes");

        builder.Property<AlertId>(nameof(AlertId));

        builder.HasIndex(nameof(AlertId));

        builder.Property(a => a.Id)
            .HasConversion<WatchModeId.EfCoreValueConverter>()
            .HasValueGeneratorFactory<WatchModeIdValueGeneratorFactory>()
            .HasColumnType("integer")
            .ValueGeneratedOnAdd()
            .UseIdentityColumn();

        builder.Property(w => w.FirstWatchDone)
            .HasColumnType("boolean")
            .IsRequired();

        builder.HasDiscriminator<char>(nameof(WatchMode))
            .HasValue<AnyChangesWatch>('A')
            .HasValue<TermWatch>('T');

        builder.Property(nameof(WatchMode))
            .HasColumnType("char")
            .IsRequired();
    }
}

public class AnyChangesWatchMapping : IEntityTypeConfiguration<AnyChangesWatch>
{
    public void Configure(EntityTypeBuilder<AnyChangesWatch> builder)
    {
        builder.Property(a => a.HtmlText)
            .HasColumnType("text");
    }
}

public class TermWatchMapping : IEntityTypeConfiguration<TermWatch>
{
    public void Configure(EntityTypeBuilder<TermWatch> builder)
    {
        builder.Property(t => t.Term)
            .HasColumnType("varchar(64)");

        builder.OwnsMany(t => t.Occurrences,
            b =>
            {
                b.ToTable("TermOccurrences");

                b.Property(o => o.Context)
                    .HasColumnType("varchar(512)");
            });

        builder.Metadata
            .FindNavigation(nameof(TermWatch.Occurrences))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}