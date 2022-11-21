using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infra.Persistence.Migrations
{
    public partial class regex_watch_mode : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Matches",
                schema: "siteWatcher_webApi",
                table: "WatchModes",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "NotifyOnDisappearance",
                schema: "siteWatcher_webApi",
                table: "WatchModes",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RegexPattern",
                schema: "siteWatcher_webApi",
                table: "WatchModes",
                type: "varchar(512)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Matches",
                schema: "siteWatcher_webApi",
                table: "WatchModes");

            migrationBuilder.DropColumn(
                name: "NotifyOnDisappearance",
                schema: "siteWatcher_webApi",
                table: "WatchModes");

            migrationBuilder.DropColumn(
                name: "RegexPattern",
                schema: "siteWatcher_webApi",
                table: "WatchModes");
        }
    }
}
