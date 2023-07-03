using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using SiteWatcher.Domain.Alerts;
using SiteWatcher.Domain.Common.ValueObjects;
using SiteWatcher.Domain.Emails;
using SiteWatcher.Domain.Notifications;
using SiteWatcher.Domain.Users;

namespace SiteWatcher.Application.Interfaces;

public interface ISiteWatcherContext
{
    EntityEntry<TEntity> Attach<TEntity>(TEntity entity) where TEntity : class;
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    DbSet<User> Users { get; }
    DbSet<Alert> Alerts { get; }
    DbSet<IdempotentConsumer> IdempotentConsumers { get; }
    DbSet<Email> Emails { get; }
    DbSet<Notification> Notifications { get; }
}