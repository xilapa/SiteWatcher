namespace SiteWatcher.Common.Repositories;
public interface IDapperQueries
{
    string GetUserByGoogleId { get; }
    string GetUserById { get; }
    string DeleteActiveUserById { get; }
    string GetSimpleAlertViewListByUserId { get; }
    string GetAlertDetails { get; }
    string DeleteUserAlert { get; }
    string SearchSimpleAlerts(int searchTermCount);
}