using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SiteWatcher.Domain.Alerts.Entities.Triggerings;
using SiteWatcher.Domain.Common.ValueObjects;

namespace SiteWatcher.Infra.Persistence.Configuration;

public class TriggeringMapping : IEntityTypeConfiguration<Triggering>
{
    public void Configure(EntityTypeBuilder<Triggering> builder)
    {
        builder.ToTable("Triggerings");

        builder.Property<AlertId>(nameof(AlertId));

        builder.Property<int>("Id");
        builder.HasKey("Id");

        builder.Property("Id")
            .HasColumnType("integer")
            .ValueGeneratedOnAdd()
            .UseIdentityColumn();

        builder.Property(t => t.Date)
            .HasColumnType("timestampz")
            .IsRequired();

        builder.Property(t => t.Status)
            .HasColumnType("smallint")
            .IsRequired();
    }
}