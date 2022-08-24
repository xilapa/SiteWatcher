using System.Linq.Expressions;
using Domain.DTOs.Alert;
using Microsoft.EntityFrameworkCore;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Domain.Models.Alerts;
using SiteWatcher.Domain.Models.Alerts.WatchModes;
using SiteWatcher.Domain.Models.Common;

namespace SiteWatcher.Infra.Repositories;

public class AlertRepository : Repository<Alert>, IAlertRepository
{
    private static readonly Func<SiteWatcherContext, AlertId, UserId, Task<UpdateAlertDto?>> GetAlertForUpdateCompiledQuery
        = EF.CompileAsyncQuery(
            (SiteWatcherContext context, AlertId alertId, UserId userId) => context.Alerts
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
                    WatchMode = alert.WatchMode
                })
                .AsSplitQuery()
                .SingleOrDefault()
        );

    public AlertRepository(SiteWatcherContext context) : base(context)
    { }

    public override Task<Alert?> GetAsync(Expression<Func<Alert, bool>> predicate, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public async Task<Alert?> GetAlertForUpdate(AlertId alertId, UserId userId)
    {
        var updateAlertDto = await GetAlertForUpdateCompiledQuery(Context, alertId, userId);

        if (updateAlertDto is null)
            return null!;

        var alert = Alert.GetModelForUpdate(updateAlertDto);
        // Attach alert and site, because the watch mode is already tracked
        Context.Attach(alert);
        return alert;
    }

    public void DeleteWatchMode(WatchModeId watchModeId)
    {
        var watchMode = Context.ChangeTracker.Entries<WatchMode>()
            .Single(w => w.Entity.Id == watchModeId);
        watchMode.State = EntityState.Deleted;
    }
}