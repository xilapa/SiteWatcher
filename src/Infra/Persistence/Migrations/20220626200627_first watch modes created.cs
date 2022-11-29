using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infra.Persistence.Migrations
{
    public partial class firstwatchmodescreated : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WatchModes",
                schema: "siteWatcher_webApi",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AlertId = table.Column<int>(type: "integer", nullable: false),
                    FirstWatchDone = table.Column<bool>(type: "boolean", nullable: false),
                    WatchMode = table.Column<char>(type: "char", nullable: false),
                    HtmlText = table.Column<string>(type: "text", nullable: true),
                    Term = table.Column<string>(type: "varchar(64)", nullable: true),
                    Active = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp", nullable: false),
                    LastUpdatedAt = table.Column<DateTime>(type: "timestamp", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WatchModes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WatchModes_Alerts_AlertId",
                        column: x => x.AlertId,
                        principalSchema: "siteWatcher_webApi",
                        principalTable: "Alerts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TermOccurrences",
                schema: "siteWatcher_webApi",
                columns: table => new
                {
                    TermWatchId = table.Column<int>(type: "integer", nullable: false),
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Context = table.Column<string>(type: "varchar(512)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TermOccurrences", x => new { x.TermWatchId, x.Id });
                    table.ForeignKey(
                        name: "FK_TermOccurrences_WatchModes_TermWatchId",
                        column: x => x.TermWatchId,
                        principalSchema: "siteWatcher_webApi",
                        principalTable: "WatchModes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WatchModes_AlertId",
                schema: "siteWatcher_webApi",
                table: "WatchModes",
                column: "AlertId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TermOccurrences",
                schema: "siteWatcher_webApi");

            migrationBuilder.DropTable(
                name: "WatchModes",
                schema: "siteWatcher_webApi");
        }
    }
}
