using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infra.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class notification_triggering_email_changes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Alerts_AlertId",
                schema: "sw",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_AlertId",
                schema: "sw",
                table: "Notifications");

            migrationBuilder.AlterColumn<DateTime>(
                name: "Date",
                schema: "sw",
                table: "Triggerings",
                type: "timestamptz",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AddColumn<short>(
                name: "Status",
                schema: "sw",
                table: "Triggerings",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                schema: "sw",
                table: "Notifications",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Active",
                schema: "sw",
                table: "Emails",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                schema: "sw",
                table: "Emails",
                type: "timestamptz",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "ErrorDate",
                schema: "sw",
                table: "Emails",
                type: "timestamptz",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastUpdatedAt",
                schema: "sw",
                table: "Emails",
                type: "timestamptz",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateTable(
                name: "NotificationAlerts",
                schema: "sw",
                columns: table => new
                {
                    NotificationId = table.Column<Guid>(type: "uuid", nullable: false),
                    AlertId = table.Column<int>(type: "integer", nullable: false),
                    TriggeringDate = table.Column<DateTime>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationAlerts", x => new { x.NotificationId, x.AlertId });
                    table.ForeignKey(
                        name: "FK_NotificationAlerts_Alerts_AlertId",
                        column: x => x.AlertId,
                        principalSchema: "sw",
                        principalTable: "Alerts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NotificationAlerts_Notifications_NotificationId",
                        column: x => x.NotificationId,
                        principalSchema: "sw",
                        principalTable: "Notifications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId",
                schema: "sw",
                table: "Notifications",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationAlerts_AlertId",
                schema: "sw",
                table: "NotificationAlerts",
                column: "AlertId");

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Users_UserId",
                schema: "sw",
                table: "Notifications",
                column: "UserId",
                principalSchema: "sw",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Users_UserId",
                schema: "sw",
                table: "Notifications");

            migrationBuilder.DropTable(
                name: "NotificationAlerts",
                schema: "sw");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_UserId",
                schema: "sw",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "Status",
                schema: "sw",
                table: "Triggerings");

            migrationBuilder.DropColumn(
                name: "UserId",
                schema: "sw",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "Active",
                schema: "sw",
                table: "Emails");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                schema: "sw",
                table: "Emails");

            migrationBuilder.DropColumn(
                name: "ErrorDate",
                schema: "sw",
                table: "Emails");

            migrationBuilder.DropColumn(
                name: "LastUpdatedAt",
                schema: "sw",
                table: "Emails");

            migrationBuilder.AlterColumn<DateTime>(
                name: "Date",
                schema: "sw",
                table: "Triggerings",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamptz");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_AlertId",
                schema: "sw",
                table: "Notifications",
                column: "AlertId");

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Alerts_AlertId",
                schema: "sw",
                table: "Notifications",
                column: "AlertId",
                principalSchema: "sw",
                principalTable: "Alerts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
