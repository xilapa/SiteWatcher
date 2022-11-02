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
            .HasColumnType("uuid");

        builder.Property(n => n.CreatedAt)
            .HasColumnType("timestamptz")
            .IsRequired();

        builder.Property(nameof(AlertId))
            .IsRequired();

        builder.HasOne(n => n.Email)
            .WithMany()
            .HasForeignKey(nameof(EmailId));

        builder.Property(nameof(EmailId))
            .IsRequired(false);
    }
}