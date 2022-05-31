namespace SiteWatcher.Infra.DapperRepositories;

public static class Queries
{
	public const string GetActiveUserByGoogleId = @"
            SELECT 
                u.""Id"", u.""Name"", u.""Email"", u.""EmailConfirmed"", u.""Language"", u.""Theme""
            FROM 
                ""siteWatcher_webApi"".""Users"" AS u
            WHERE
                u.""GoogleId"" = @googleId AND u.""Active"" ";
}