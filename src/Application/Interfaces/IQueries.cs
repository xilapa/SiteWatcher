namespace SiteWatcher.Application.Interfaces;
public interface IQueries
{
    string GetUserByGoogleId { get; }
    string GetUserById { get; }
    string GetSimpleAlertViewListByUserId { get; }
    string GetAlertDetails { get; }
    string DeleteUserAlert { get; }
    string SearchSimpleAlerts(int searchTermCount);
}