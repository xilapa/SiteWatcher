using SiteWatcher.Domain.Common.ValueObjects;

namespace SiteWatcher.Application.Common.Queries;
public interface IQueries
{
    QueryResult GetUserByGoogleId(string googleId);
    QueryResult GetUserById(UserId userId);
    QueryResult GetSimpleAlertViewListByUserId(UserId userId, AlertId? lastAlertId, int take);
    QueryResult GetAlertDetails(UserId userId, AlertId alertId);
    QueryResult SearchSimpleAlerts(UserId userId, string[] searchTerms, int take);
}