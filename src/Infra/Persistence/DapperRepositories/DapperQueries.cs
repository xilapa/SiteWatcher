using SiteWatcher.Application.Interfaces;

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
}