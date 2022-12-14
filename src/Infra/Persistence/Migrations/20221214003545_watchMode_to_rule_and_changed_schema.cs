using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infra.Persistence.Migrations
{
    public partial class watchMode_to_rule_and_changed_schema : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TermOccurrences_WatchModes_TermWatchId",
                schema: "siteWatcher_webApi",
                table: "TermOccurrences");

            migrationBuilder.DropTable(
                name: "WatchModes",
                schema: "siteWatcher_webApi");

            migrationBuilder.EnsureSchema(
                name: "sw");

            migrationBuilder.RenameTable(
                name: "Users",
                schema: "siteWatcher_webApi",
                newName: "Users",
                newSchema: "sw");

            migrationBuilder.RenameTable(
                name: "TermOccurrences",
                schema: "siteWatcher_webApi",
                newName: "TermOccurrences",
                newSchema: "sw");

            migrationBuilder.RenameTable(
                name: "Notifications",
                schema: "siteWatcher_webApi",
                newName: "Notifications",
                newSchema: "sw");

            migrationBuilder.RenameTable(
                name: "IdempotentConsumers",
                schema: "worker",
                newName: "IdempotentConsumers",
                newSchema: "sw");

            migrationBuilder.RenameTable(
                name: "Emails",
                schema: "siteWatcher_webApi",
                newName: "Emails",
                newSchema: "sw");

            migrationBuilder.RenameTable(
                name: "Alerts",
                schema: "siteWatcher_webApi",
                newName: "Alerts",
                newSchema: "sw");

            migrationBuilder.RenameColumn(
                name: "TermWatchId",
                schema: "sw",
                table: "TermOccurrences",
                newName: "TermRuleId");

            migrationBuilder.CreateTable(
                name: "Rules",
                schema: "sw",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FirstWatchDone = table.Column<bool>(type: "boolean", nullable: false),
                    AlertId = table.Column<int>(type: "integer", nullable: false),
                    Rule = table.Column<char>(type: "char", nullable: false),
                    HtmlHash = table.Column<string>(type: "varchar(64)", nullable: true),
                    NotifyOnDisappearance = table.Column<bool>(type: "boolean", nullable: true),
                    RegexPattern = table.Column<string>(type: "varchar(512)", nullable: true),
                    Matches = table.Column<string>(type: "text", nullable: true),
                    Term = table.Column<string>(type: "varchar(64)", nullable: true),
                    Active = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    LastUpdatedAt = table.Column<DateTime>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Rules_Alerts_AlertId",
                        column: x => x.AlertId,
                        principalSchema: "sw",
                        principalTable: "Alerts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Rules_AlertId",
                schema: "sw",
                table: "Rules",
                column: "AlertId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_TermOccurrences_Rules_TermRuleId",
                schema: "sw",
                table: "TermOccurrences",
                column: "TermRuleId",
                principalSchema: "sw",
                principalTable: "Rules",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TermOccurrences_Rules_TermRuleId",
                schema: "sw",
                table: "TermOccurrences");

            migrationBuilder.DropTable(
                name: "Rules",
                schema: "sw");

            migrationBuilder.EnsureSchema(
                name: "siteWatcher_webApi");

            migrationBuilder.EnsureSchema(
                name: "worker");

            migrationBuilder.RenameTable(
                name: "Users",
                schema: "sw",
                newName: "Users",
                newSchema: "siteWatcher_webApi");

            migrationBuilder.RenameTable(
                name: "TermOccurrences",
                schema: "sw",
                newName: "TermOccurrences",
                newSchema: "siteWatcher_webApi");

            migrationBuilder.RenameTable(
                name: "Notifications",
                schema: "sw",
                newName: "Notifications",
                newSchema: "siteWatcher_webApi");

            migrationBuilder.RenameTable(
                name: "IdempotentConsumers",
                schema: "sw",
                newName: "IdempotentConsumers",
                newSchema: "worker");

            migrationBuilder.RenameTable(
                name: "Emails",
                schema: "sw",
                newName: "Emails",
                newSchema: "siteWatcher_webApi");

            migrationBuilder.RenameTable(
                name: "Alerts",
                schema: "sw",
                newName: "Alerts",
                newSchema: "siteWatcher_webApi");

            migrationBuilder.RenameColumn(
                name: "TermRuleId",
                schema: "siteWatcher_webApi",
                table: "TermOccurrences",
                newName: "TermWatchId");

            migrationBuilder.CreateTable(
                name: "WatchModes",
                schema: "siteWatcher_webApi",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Active = table.Column<bool>(type: "boolean", nullable: false),
                    AlertId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    FirstWatchDone = table.Column<bool>(type: "boolean", nullable: false),
                    LastUpdatedAt = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    WatchMode = table.Column<char>(type: "char", nullable: false),
                    HtmlHash = table.Column<string>(type: "varchar(64)", nullable: true),
                    NotifyOnDisappearance = table.Column<bool>(type: "boolean", nullable: true),
                    RegexPattern = table.Column<string>(type: "varchar(512)", nullable: true),
                    Matches = table.Column<string>(type: "text", nullable: true),
                    Term = table.Column<string>(type: "varchar(64)", nullable: true)
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

            migrationBuilder.CreateIndex(
                name: "IX_WatchModes_AlertId",
                schema: "siteWatcher_webApi",
                table: "WatchModes",
                column: "AlertId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_TermOccurrences_WatchModes_TermWatchId",
                schema: "siteWatcher_webApi",
                table: "TermOccurrences",
                column: "TermWatchId",
                principalSchema: "siteWatcher_webApi",
                principalTable: "WatchModes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
