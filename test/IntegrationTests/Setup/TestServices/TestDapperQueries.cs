using SiteWatcher.Infra.DapperRepositories;

namespace SiteWatcher.IntegrationTests.Setup.TestServices;

public class SqliteDapperQueries : DapperQueries
{
    public SqliteDapperQueries()
    {
        GetUserByGoogleId = RemoveSchema(base.GetUserByGoogleId);
        DeleteActiveUserById = RemoveSchema(base.DeleteActiveUserById);
        GetSimpleAlertViewListByUserId = RemoveSchema(base.GetSimpleAlertViewListByUserId);
        GetAlertDetails = RemoveSchema(base.GetAlertDetails);
        DeleteUserAlert = RemoveSchema(base.DeleteUserAlert);
    }

    public override string GetUserByGoogleId { get; }
    public override string DeleteActiveUserById { get; }
    public override string GetSimpleAlertViewListByUserId { get; }
    public override string GetAlertDetails { get; }
    public override string DeleteUserAlert { get; }

    private static string RemoveSchema(string query) =>
        query.Replace(@"""sw"".", "");
}