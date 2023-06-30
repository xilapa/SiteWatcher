using SiteWatcher.Infra.DapperRepositories;

namespace SiteWatcher.IntegrationTests.Setup.TestServices;

public class SqliteDapperQueries : PostgresQueries
{
    public SqliteDapperQueries()
    {
        GetUserByGoogleId = RemoveSchema(base.GetUserByGoogleId);
        GetSimpleAlertViewListByUserId = RemoveSchema(base.GetSimpleAlertViewListByUserId);
        GetAlertDetails = RemoveSchema(base.GetAlertDetails);
        DeleteUserAlert = RemoveSchema(base.DeleteUserAlert);
        GetUserById = RemoveSchema(base.GetUserById);
    }

    public override string GetUserByGoogleId { get; }
    public override string GetUserById { get; }
    public override string GetSimpleAlertViewListByUserId { get; }
    public override string GetAlertDetails { get; }
    public override string DeleteUserAlert { get; }

    private static string RemoveSchema(string query) =>
        query.Replace(@"""sw"".", "");
}