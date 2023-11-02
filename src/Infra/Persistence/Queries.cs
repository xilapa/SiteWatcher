using System.Collections.Concurrent;
using SiteWatcher.Application.Common.Queries;
using SiteWatcher.Domain.Common;
using SiteWatcher.Domain.Common.ValueObjects;

namespace SiteWatcher.Infra.Persistence;

public class Queries : IQueries
{
    private readonly ConcurrentDictionary<int, string> _searchSimpleAlertsQueryCache;

    public Queries(DatabaseType dbType)
    {
        _searchSimpleAlertsQueryCache = new ConcurrentDictionary<int, string>();
        if (dbType is DatabaseType.SqliteInMemory or DatabaseType.SqliteOnDisk)
            PrepareSqliteQueries();
    }

    private void PrepareSqliteQueries()
    {
        string RemoveSchema(string query) => query.Replace(@"""sw"".", "");

        GetUserByGoogleIdQuery = RemoveSchema(GetUserByGoogleIdQuery);
        GetUserByIdQuery = RemoveSchema(GetUserByIdQuery);
        GetSimpleAlertViewListByUserIdQuery = RemoveSchema(GetSimpleAlertViewListByUserIdQuery);
        GetAlertDetailsQuery = RemoveSchema(GetAlertDetailsQuery);
    }

    #region Raw queries

    private string GetUserByGoogleIdQuery = @"
        SELECT 
            u.""Id"", u.""Active"", u.""Name"", u.""Email"", u.""EmailConfirmed"", u.""Language"", u.""Theme""
        FROM 
            ""sw"".""Users"" AS u
        WHERE
            u.""GoogleId"" = @googleId ";

    private string GetUserByIdQuery = @"
            SELECT 
                u.""Id"", u.""Active"", u.""Name"", u.""Email"", u.""EmailConfirmed"", u.""Language"", u.""Theme""
            FROM 
                ""sw"".""Users"" AS u
            WHERE
                u.""Id"" = @userId ";

    private string GetSimpleAlertViewListByUserIdQuery = @"
            SELECT 
                COUNT(a.""Id"") 
            FROM 
                ""sw"".""Alerts"" a
            WHERE
                a.""UserId"" = @userId;
            SELECT 
                a.""Id"",
	            a.""Name"",
	            a.""CreatedAt"",
                a.""Frequency"",
                a.""LastVerification"",
                a.""Site_Name"" SiteName,
                r.""Rule"" RuleType,
                COUNT(t.""Id"") TriggeringsCount
            FROM 
            ""sw"".""Alerts"" a
                INNER JOIN ""sw"".""Rules"" r 
                    ON a.""Id"" = r.""AlertId""
                LEFT JOIN ""sw"".""Triggerings"" t
                    ON a.""Id"" = t.""AlertId""
            WHERE
                a.""Id"" > @lastAlertId
                AND a.""UserId"" = @userId
            GROUP BY
            	1,2,3,4,5,6,7
            ORDER BY 
                a.""Id""
            LIMIT @take";

    private string GetAlertDetailsQuery = @"
            SELECT
                a.""Id"",
                a.""Site_Uri"" SiteUri,
                r.""Id"" RuleId,
                r.""Term"",
                r.""RegexPattern"",
                r.""NotifyOnDisappearance""
            FROM 
            ""sw"".""Alerts"" a
                INNER JOIN ""sw"".""Rules"" r 
                ON a.""Id"" = r.""AlertId""
            WHERE
                a.""Id"" = @alertId
                AND a.""UserId"" = @userId";

    #endregion

    public DbQuery GetUserByGoogleId(string googleId)
    {
        var parameters = new Dictionary<string, object> { { "googleId", googleId } };
        return new DbQuery(GetUserByGoogleIdQuery, parameters);
    }

    public DbQuery GetUserById(UserId userId)
    {
        var parameters = new Dictionary<string, object> { { "userId", userId.Value } };
        return new DbQuery(GetUserByIdQuery, parameters);
    }

    public DbQuery GetSimpleAlertViewListByUserId(UserId userId, AlertId? lastAlertId, int take)
    {
        var parameters = new Dictionary<string, object>
        {
            { "userId", userId.Value },
            { "lastAlertId", lastAlertId?.Value ?? 0 },
            { "take", take }
        };
        return new DbQuery(GetSimpleAlertViewListByUserIdQuery, parameters);
    }

    public DbQuery GetAlertDetails(UserId userId, AlertId alertId)
    {
        var parameters = new Dictionary<string, object>
        {
            { "userId", userId.Value },
            { "alertId", alertId.Value }
        };
        return new DbQuery(GetAlertDetailsQuery, parameters);
    }

    public DbQuery SearchSimpleAlerts(UserId userId, string[] searchTerms, int take)
    {
        var query = _searchSimpleAlertsQueryCache.GetOrAdd(searchTerms.Length, GenerateSearchSimpleAlertsQuery);
        var parameters = new Dictionary<string, object> {{"userId", userId}};

        for (var i = 0; i < searchTerms.Length; i++)
        {
            parameters.Add($"searchTermWildCards{i}", $"%{searchTerms[i]}%");
            parameters.Add($"searchTerm{i}", searchTerms[i]);
        }
        return new DbQuery(query, parameters);
    }

    private static string GenerateSearchSimpleAlertsQuery(int searchTermsLenght)
    {
        var whereQueryBuilder = StringBuilderCache.Acquire(200);
        var orderByQueryBuilder = StringBuilderCache.Acquire(200);
        foreach (var termIndex in Enumerable.Range(0, searchTermsLenght))
        {
            whereQueryBuilder.Append(@"(""SearchField"" LIKE @searchTermWildCards").Append(termIndex).Append(')');
            orderByQueryBuilder.Append(@" ""similarity""(""SearchField"", @searchTerm").Append(termIndex).Append(") ");

            if (termIndex == searchTermsLenght - 1) continue;
            whereQueryBuilder.Append(" OR ");
            orderByQueryBuilder.Append('+');
        }

        return $@"
            SELECT 
                a.""Id"",
	            a.""Name"",
	            a.""CreatedAt"",
                a.""Frequency"",
                a.""LastVerification"",
                a.""Site_Name"" SiteName,
                r.""Rule"" RuleType
            FROM 
                ""sw"".""Alerts"" a
                    INNER JOIN ""sw"".""Rules"" r 
                        ON a.""Id"" = r.""AlertId""
            WHERE
                a.""UserId"" = @userId
                AND ( {StringBuilderCache.GetStringAndRelease(whereQueryBuilder)} )
            ORDER BY 
                {StringBuilderCache.GetStringAndRelease(orderByQueryBuilder)}
            DESC
            LIMIT 10";
    }
}