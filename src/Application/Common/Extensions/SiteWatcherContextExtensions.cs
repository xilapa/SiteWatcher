using Microsoft.EntityFrameworkCore;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Domain.Alerts.Enums;
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
                        || a.LastVerification < currentTime.AddHours(-2)
                    )
                )
            )
            .ThenInclude(a => a.Rule)
            .Take(take)
            .ToArrayAsync(ct);
    }
}