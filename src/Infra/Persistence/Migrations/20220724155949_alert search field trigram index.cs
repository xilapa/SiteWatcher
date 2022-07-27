﻿using Microsoft.EntityFrameworkCore.Migrations;
using SiteWatcher.Infra;

#nullable disable

namespace Infra.Persistence.Migrations
{
    public partial class alertsearchfieldtrigramindex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(ManualMigrations.AlertSearchTrigramIndex);
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
