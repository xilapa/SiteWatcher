using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SiteWatcher.Infra.Migrations
{
    public partial class ThemecolumnonUser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<short>(
                name: "Theme",
                schema: "siteWatcher_webApi",
                table: "Users",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Theme",
                schema: "siteWatcher_webApi",
                table: "Users");
        }
    }
}
