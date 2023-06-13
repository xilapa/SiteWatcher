using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using SiteWatcher.Domain.Alerts;
using SiteWatcher.Domain.Alerts.DTOs;
using SiteWatcher.Domain.Alerts.Repositories;
using SiteWatcher.Domain.Common.ValueObjects;

namespace SiteWatcher.Infra.Repositories;

public class AlertRepository : Repository<Alert>, IAlertRepository
{
    private readonly Func<SiteWatcherContext, AlertId, UserId, Task<UpdateAlertDto?>> GetAlertForUpdateCompiledQuery
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
                    Rule = alert.Rule,
                    LastVerification = alert.LastVerification
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
        // Attach alert and site, because the rule is already tracked
        Context.Attach(alert);
        return alert;
    }
}