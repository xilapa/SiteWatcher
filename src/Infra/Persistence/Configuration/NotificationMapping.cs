using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SiteWatcher.Domain.Alerts;
using SiteWatcher.Domain.Common.ValueObjects;
using SiteWatcher.Domain.Notifications;

namespace SiteWatcher.Infra.Persistence.Configuration;

public class NotificationMapping : BaseModelMapping<Notification, NotificationId>
{
    public override void Configure(EntityTypeBuilder<Notification> builder)
    {
        base.Configure(builder);

        builder.ToTable("Notifications");

        builder.Property(n => n.Id)
            .HasConversion<NotificationId.EfCoreValueConverter>()
            .HasColumnType("uuid");

        builder.HasOne(n => n.Email)
            .WithMany()
            .HasForeignKey(n => n.EmailId);

        builder.Property(n => n.EmailId)
            .IsRequired(false);

        builder.HasOne(n => n.User)
            .WithMany()
            .HasForeignKey(n => n.UserId);

        // N-N relationship between Alert and Notification with skip navigation
        builder.HasMany(n => n.Alerts)
            .WithMany(a => a.Notifications)
            .UsingEntity<NotificationAlert>(cfg =>
            {
                cfg.ToTable("NotificationAlerts");
                cfg.HasKey(an => new { an.AlertId, an.NotificationId });
                cfg.HasOne<Notification>()
                    .WithMany(an => an.NotificationAlerts)
                    .HasForeignKey(an => an.NotificationId);
                cfg.HasOne<Alert>()
                    .WithMany()
                    .HasForeignKey(an => an.AlertId);
            });

        builder.Metadata
            .FindNavigation(nameof(Notification.NotificationAlerts))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}