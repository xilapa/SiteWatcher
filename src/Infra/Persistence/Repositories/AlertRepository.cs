using System.Linq.Expressions;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Domain.Models.Alerts;

namespace SiteWatcher.Infra.Repositories;

public class AlertRepository : Repository<Alert>, IAlertRepository
{
    public AlertRepository(SiteWatcherContext context) : base(context) { }

    public override Task<Alert?> GetAsync(Expression<Func<Alert, bool>> predicate, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}