using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SiteWatcher.Domain.Alerts;
using SiteWatcher.Domain.Common.ValueObjects;
using SiteWatcher.Domain.Notifications;

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

        builder.Property(m => m.CreatedAt)
            .HasColumnType("timestamptz")
            .IsRequired();

        builder.HasOne(n => n.Email)
            .WithMany()
            .HasForeignKey(n => n.EmailId);

        builder.Property(n => n.EmailId)
            .IsRequired(false);

        builder.HasOne(n => n.User)
            .WithMany()
            .HasForeignKey(n => n.UserId)
            .IsRequired();

        builder.HasMany(n => n.Alerts)
            .WithMany(a => a.Notifications)
            .UsingEntity<NotificationAlert>(cfg =>
            {
                cfg.ToTable("NotificationAlerts");
                cfg.HasKey(na => new { na.NotificationId, na.AlertId });
                cfg.HasOne<Notification>()
                    .WithMany(n => n.NotificationAlerts)
                    .HasForeignKey(na => na.NotificationId);
                cfg.HasOne<Alert>()
                    .WithMany()
                    .HasForeignKey(an => an.AlertId);
            });

        builder.Metadata
            .FindNavigation(nameof(Notification.NotificationAlerts))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}