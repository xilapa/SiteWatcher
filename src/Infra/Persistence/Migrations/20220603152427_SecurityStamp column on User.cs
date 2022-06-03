using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SiteWatcher.Infra.Migrations
{
    public partial class SecurityStampcolumnonUser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SecurityStamp",
                schema: "siteWatcher_webApi",
                table: "Users",
                type: "varchar(120)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_User_SecurityStamp",
                schema: "siteWatcher_webApi",
                table: "Users",
                column: "SecurityStamp");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_User_SecurityStamp",
                schema: "siteWatcher_webApi",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "SecurityStamp",
                schema: "siteWatcher_webApi",
                table: "Users");
        }
    }
}
