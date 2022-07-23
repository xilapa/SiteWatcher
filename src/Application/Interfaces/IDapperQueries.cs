namespace SiteWatcher.Application.Interfaces;

public interface IDapperQueries
{
    string GetUserByGoogleId { get; }
    string DeleteActiveUserById { get; }
    string GetSimpleAlertViewListByUserId { get; }
    string GetAlertDetails { get; }
    string DeleteUserAlert { get; }
    string SearchSimpleAlerts(int searchTermCount);
}