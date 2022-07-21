using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infra.Persistence.Migrations
{
    public partial class searchcolumnaddedtoalert : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NameSearch",
                schema: "siteWatcher_webApi",
                table: "Alerts",
                type: "varchar(64)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Alerts_NameSearch",
                schema: "siteWatcher_webApi",
                table: "Alerts",
                column: "NameSearch");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Alerts_NameSearch",
                schema: "siteWatcher_webApi",
                table: "Alerts");

            migrationBuilder.DropColumn(
                name: "NameSearch",
                schema: "siteWatcher_webApi",
                table: "Alerts");
        }
    }
}
