using SiteWatcher.Infra.DapperRepositories;

namespace SiteWatcher.IntegrationTests.Setup.TestServices;

public class SqliteDapperQueries : DapperQueries
{
    public SqliteDapperQueries()
    {
        GetUserByGoogleId = RemoveSchema(base.GetUserByGoogleId);
        DeleteActiveUserById = RemoveSchema(base.DeleteActiveUserById);
    }

    public override string GetUserByGoogleId { get; }
    public override string DeleteActiveUserById { get; }

    private static string RemoveSchema(string query) =>
        query.Replace(@"""siteWatcher_webApi"".", "");
}