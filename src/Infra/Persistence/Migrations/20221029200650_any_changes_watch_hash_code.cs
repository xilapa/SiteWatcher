using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infra.Persistence.Migrations
{
    public partial class any_changes_watch_hash_code : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HtmlText",
                schema: "siteWatcher_webApi",
                table: "WatchModes");

            migrationBuilder.AddColumn<int>(
                name: "HtmlHash",
                schema: "siteWatcher_webApi",
                table: "WatchModes",
                type: "integer",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HtmlHash",
                schema: "siteWatcher_webApi",
                table: "WatchModes");

            migrationBuilder.AddColumn<string>(
                name: "HtmlText",
                schema: "siteWatcher_webApi",
                table: "WatchModes",
                type: "text",
                nullable: true);
        }
    }
}
