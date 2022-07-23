using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infra.Persistence.Migrations
{
    public partial class indexesforunnaccentsearch : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                CREATE EXTENSION unaccent;

                -- IMMUTABLE SQL wrapper arround unnaccent
                -- https://stackoverflow.com/questions/11005036/does-postgresql-support-accent-insensitive-collations
                CREATE OR REPLACE FUNCTION public.immutable_unaccent(regdictionary, text)
                  RETURNS text LANGUAGE c IMMUTABLE PARALLEL SAFE STRICT AS
                '$libdir/unaccent', 'unaccent_dict';

                CREATE OR REPLACE FUNCTION public.f_unaccent(text)
                  RETURNS text LANGUAGE sql IMMUTABLE PARALLEL SAFE STRICT AS
                $func$
                SELECT public.immutable_unaccent(regdictionary 'public.unaccent', $1)
                $func$;

                -- Index on expression
                -- https://www.postgresql.org/docs/current/indexes-expressional.html
                CREATE INDEX IX_Alerts_Name_UNACCENT ON ""siteWatcher_webApi"".""Alerts""(public.f_unaccent(""Name""));
                CREATE INDEX IX_Alerts_Site_Name_UNACCENT ON ""siteWatcher_webApi"".""Alerts""(public.f_unaccent(""Site_Name""));
                CREATE INDEX IX_Alerts_Site_Uri_UNACCENT ON ""siteWatcher_webApi"".""Alerts""(public.f_unaccent(""Site_Uri""));
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DROP INDEX ""siteWatcher_webApi"".""ix_alerts_name_unaccent"";
                DROP EXTENSION unaccent;");
        }
    }
}
