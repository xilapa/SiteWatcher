using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SiteWatcher.Domain.Models.Alerts;
using SiteWatcher.Domain.Models.Common;

namespace SiteWatcher.Infra.Persistence.Configuration;

public class NotificationMapping : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("Notifications");

        builder.HasKey(n => n.Id);

        builder.Property(n => n.Id)
            .HasConversion<NotificationId.EfCoreValueConverter>()
            .HasValueGeneratorFactory<NotificationIdValueGeneratorFactory>()
            .HasColumnType("integer")
            .ValueGeneratedOnAdd()
            .UseIdentityColumn();

        builder.Property(n => n.CreatedAt)
            .HasColumnType("timestamptz")
            .IsRequired();
    }
}