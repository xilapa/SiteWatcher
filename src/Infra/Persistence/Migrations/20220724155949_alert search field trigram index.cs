using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infra.Persistence.Migrations
{
    public partial class alertsearchfieldtrigramindex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                -- https://www.postgresql.org/docs/current/pgtrgm.html
                CREATE EXTENSION pg_trgm;
                -- More about indexes for like queries https://niallburkley.com/blog/index-columns-for-like-in-postgres/
                CREATE INDEX IX_Alert_SearchField_TRIGRAM ON ""siteWatcher_webApi"".""Alerts"" USING gin(""SearchField"" gin_trgm_ops);
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                 DROP INDEX ""siteWatcher_webApi"".IX_Alert_SearchField_TRIGRAM;
                DROP EXTENSION pg_trgm;
            ");
        }
    }
}
