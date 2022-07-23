using SiteWatcher.Application.Interfaces;
using SiteWatcher.Domain.Utils;

namespace SiteWatcher.Infra.DapperRepositories;

public class DapperQueries : IDapperQueries
{
	public virtual string GetUserByGoogleId => @"
            SELECT 
                u.""Id"", u.""Active"", u.""Name"", u.""Email"", u.""EmailConfirmed"", u.""Language"", u.""Theme""
            FROM 
                ""siteWatcher_webApi"".""Users"" AS u
            WHERE
                u.""GoogleId"" = @googleId ";

    public virtual string DeleteActiveUserById => @"
            DELETE 
            FROM 
                ""siteWatcher_webApi"".""Users"" AS u
            WHERE
                u.""Id"" = @userId AND u.""Active"" ";

    public virtual string GetSimpleAlertViewListByUserId => @"
            SELECT 
                COUNT(a.""Id"") 
            FROM 
                ""siteWatcher_webApi"".""Alerts"" a
            WHERE
                a.""UserId"" = @userId;

            SELECT 
                a.""Id"",
	            a.""Name"",
	            a.""CreatedAt"",
                a.""Frequency"",
                a.""LastVerification"",
                a.""Site_Name"" SiteName,
                wm.""WatchMode""
            FROM 
            ""siteWatcher_webApi"".""Alerts"" a
                INNER JOIN ""siteWatcher_webApi"".""WatchModes"" wm 
                    ON a.""Id"" = wm.""AlertId""
            WHERE
                a.""Id"" > @lastAlertId
                AND a.""UserId"" = @userId
            ORDER BY 
                a.""Id""
            LIMIT @take";

    public virtual string GetAlertDetails => @"
            SELECT
                a.""Id"",
                a.""Site_Uri"" SiteUri,
                wm.""Id"" WatchModeId,
                wm.""Term""
            FROM 
            ""siteWatcher_webApi"".""Alerts"" a
                INNER JOIN ""siteWatcher_webApi"".""WatchModes"" wm 
                ON a.""Id"" = wm.""AlertId""
            WHERE
                a.""Id"" = @alertId
                AND a.""UserId"" = @userId";

    public virtual string DeleteUserAlert => @"
            DELETE 
            FROM 
	            ""siteWatcher_webApi"".""Alerts""
            WHERE	
                ""Id"" = @alertId
                AND ""UserId"" =  @userId";

    public virtual string SearchSimpleAlerts(int searchTermCount)
    {
        var stringBuilder = StringBuilderCache.Acquire();
        foreach (var termIndex in Enumerable.Range(0, searchTermCount))
        {
            stringBuilder.Append(@"(
                    f_unaccent(a.""Name"") ILIKE @searchTerm").Append(termIndex).Append(@" OR
                    f_unaccent(a.""Site_Name"") ILIKE @searchTerm").Append(termIndex).Append(@" OR
                    f_unaccent(a.""Site_Uri"") ILIKE @searchTerm").Append(termIndex).Append(@"
                     )");

            if (termIndex != searchTermCount - 1)
                stringBuilder.Append(" OR ");
        }

        var baseQuery = $@"
            SELECT 
                a.""Id"",
	            a.""Name"",
	            a.""CreatedAt"",
                a.""Frequency"",
                a.""LastVerification"",
                a.""Site_Name"" SiteName,
                wm.""WatchMode""
            FROM 
                ""siteWatcher_webApi"".""Alerts"" a
                    INNER JOIN ""siteWatcher_webApi"".""WatchModes"" wm 
                        ON a.""Id"" = wm.""AlertId""
            WHERE
                a.""UserId"" = @userId
                AND ( {StringBuilderCache.GetStringAndRelease(stringBuilder)} )
            ORDER BY 
                a.""Id""
            LIMIT @take";
        return baseQuery;
    }
}