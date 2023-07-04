using Microsoft.EntityFrameworkCore;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Domain.Alerts;
using SiteWatcher.Domain.Alerts.DTOs;
using SiteWatcher.Domain.Alerts.Enums;
using SiteWatcher.Domain.Common.ValueObjects;
using SiteWatcher.Domain.Users;

namespace SiteWatcher.Application.Common.Extensions;

public static class SiteWatcherContextExtensions
{
    public static async Task<User[]> GetUserWithPendingAlertsAsync(this ISiteWatcherContext ctx, DateTime? lastCreatedAt,
        List<Frequencies> freqs, int take, DateTime currentTime, CancellationToken ct)
    {
        return await ctx.Users
            .OrderBy(_ => _.CreatedAt)
            .Where(u =>
                u.Active
                && u.EmailConfirmed
                && (!lastCreatedAt.HasValue ||  u.CreatedAt > lastCreatedAt))
            .Include(u =>
                u.Alerts.Where(a =>
                    freqs.Contains(a.Frequency)
                    && (
                        a.LastVerification == null
                        || a.LastVerification <= currentTime.AddHours(-2)
                    )
                )
            )
            .ThenInclude(a => a.Rule)
            .Take(take)
            .ToArrayAsync(ct);
    }

    public static async Task<Alert?> GetAlertForUpdateAsync(this ISiteWatcherContext ctx, AlertId alertId, UserId userId,
        CancellationToken ct)
    {
        var alertDto = await ctx.Alerts
            .Where(a => a.Id == alertId && a.UserId == userId && a.Active)
            .Select(alert => new UpdateAlertDto
            {
                Id = alert.Id,
                UserId = alert.UserId,
                CreatedAt = alert.CreatedAt,
                Name = alert.Name,
                Frequency = alert.Frequency,
                SiteName = alert.Site.Name,
                SiteUri = alert.Site.Uri,
                Rule = alert.Rule,
                LastVerification = alert.LastVerification
            })
            .SingleOrDefaultAsync(ct);

        if (alertDto is null)
            return null;

        var alert = Alert.GetModelForUpdate(alertDto);
        // Attach alert and site, because the rule is already tracked
        ctx.Attach(alert);
        return alert;
    }

    public static async Task<bool> HasBeenProcessed(this ISiteWatcherContext context, string messageId, string consumerName) =>
        await context.IdempotentConsumers
            .AsNoTracking()
            .AnyAsync(i => i.Consumer == consumerName && i.MessageId == messageId);

    public static void MarkMessageAsConsumed(this ISiteWatcherContext context, string messageId, string consumerName)
    {
        var idemPotencyConsumer = new IdempotentConsumer
        {
            MessageId = messageId,
            Consumer = consumerName,
        };
        context.IdempotentConsumers.Add(idemPotencyConsumer);
    }
}