using SiteWatcher.Application.Interfaces;
using SiteWatcher.Domain.Common;

namespace SiteWatcher.Infra.DapperRepositories;

public class PostgresQueries : IQueries
{
	public virtual string GetUserByGoogleId => @"
            SELECT 
                u.""Id"", u.""Active"", u.""Name"", u.""Email"", u.""EmailConfirmed"", u.""Language"", u.""Theme""
            FROM 
                ""sw"".""Users"" AS u
            WHERE
                u.""GoogleId"" = @googleId ";

    public virtual string GetUserById => @"
            SELECT 
                u.""Id"", u.""Active"", u.""Name"", u.""Email"", u.""EmailConfirmed"", u.""Language"", u.""Theme""
            FROM 
                ""sw"".""Users"" AS u
            WHERE
                u.""Id"" = @id ";

    public virtual string GetSimpleAlertViewListByUserId => @"
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
                r.""Rule"",
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

    public virtual string GetAlertDetails => @"
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

    public virtual string DeleteUserAlert => @"
            DELETE 
            FROM 
	            ""sw"".""Alerts""
            WHERE	
                ""Id"" = @alertId
                AND ""UserId"" =  @userId";

    public virtual string SearchSimpleAlerts(int searchTermCount)
    {
        var whereQueryBuilder = StringBuilderCache.Acquire(200);
        var orderByQueryBuilder = StringBuilderCache.Acquire(200);
        orderByQueryBuilder.Append('(');
        foreach (var termIndex in Enumerable.Range(0, searchTermCount))
        {
            whereQueryBuilder.Append(@"(""SearchField"" LIKE @searchTermWildCards").Append(termIndex).Append(')');
            orderByQueryBuilder.Append(@" ""similarity""(""SearchField"", @searchTerm").Append(termIndex).Append(") ");

            if (termIndex == searchTermCount - 1) continue;
            whereQueryBuilder.Append(" OR ");
            orderByQueryBuilder.Append('+');
        }
        orderByQueryBuilder.Append(')');

        return $@"
            SELECT 
                a.""Id"",
	            a.""Name"",
	            a.""CreatedAt"",
                a.""Frequency"",
                a.""LastVerification"",
                a.""Site_Name"" SiteName,
                wm.""Rule""
            FROM 
                ""sw"".""Alerts"" a
                    INNER JOIN ""sw"".""Rules"" wm 
                        ON a.""Id"" = wm.""AlertId""
            WHERE
                a.""UserId"" = @userId
                AND ( {StringBuilderCache.GetStringAndRelease(whereQueryBuilder)} )
            ORDER BY 
                {StringBuilderCache.GetStringAndRelease(orderByQueryBuilder)}
            DESC
            LIMIT @take";
    }
}