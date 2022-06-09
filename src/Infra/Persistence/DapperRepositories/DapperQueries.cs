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
}