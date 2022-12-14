namespace SiteWatcher.Infra;

public static class ManualMigrations
{
    public static string AlertSearchTrigramIndex = @"
                -- https://www.postgresql.org/docs/current/pgtrgm.html
                CREATE EXTENSION pg_trgm;
                -- More about indexes for like queries https://niallburkley.com/blog/index-columns-for-like-in-postgres/
                CREATE INDEX IX_Alert_SearchField_TRIGRAM ON ""sw"".""Alerts"" USING gin(""SearchField"" gin_trgm_ops);
    ";
}