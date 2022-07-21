using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infra.Persistence.Migrations
{
    public partial class searchcolumnaddedtosite : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Site_NameSearch",
                schema: "siteWatcher_webApi",
                table: "Alerts",
                type: "varchar(64)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Site_UriSearch",
                schema: "siteWatcher_webApi",
                table: "Alerts",
                type: "varchar(512)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Alerts_Site_NameSearch",
                schema: "siteWatcher_webApi",
                table: "Alerts",
                column: "Site_NameSearch");

            migrationBuilder.CreateIndex(
                name: "IX_Alerts_Site_UriSearch",
                schema: "siteWatcher_webApi",
                table: "Alerts",
                column: "Site_UriSearch");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Alerts_Site_NameSearch",
                schema: "siteWatcher_webApi",
                table: "Alerts");

            migrationBuilder.DropIndex(
                name: "IX_Alerts_Site_UriSearch",
                schema: "siteWatcher_webApi",
                table: "Alerts");

            migrationBuilder.DropColumn(
                name: "Site_NameSearch",
                schema: "siteWatcher_webApi",
                table: "Alerts");

            migrationBuilder.DropColumn(
                name: "Site_UriSearch",
                schema: "siteWatcher_webApi",
                table: "Alerts");
        }
    }
}
