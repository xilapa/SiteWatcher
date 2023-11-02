using SiteWatcher.Domain.Common.ValueObjects;

namespace SiteWatcher.Application.Common.Queries;
public interface IQueries
{
    DbQuery GetUserByGoogleId(string googleId);
    DbQuery GetUserById(UserId userId);
    DbQuery GetSimpleAlertViewListByUserId(UserId userId, AlertId? lastAlertId, int take);
    DbQuery GetAlertDetails(UserId userId, AlertId alertId);
    DbQuery SearchSimpleAlerts(UserId userId, string[] searchTerms, int take);
}