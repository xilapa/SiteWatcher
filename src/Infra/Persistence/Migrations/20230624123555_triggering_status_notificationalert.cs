using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infra.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class triggering_status_notificationalert : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Users_UserId",
                schema: "sw",
                table: "Notifications");

            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                schema: "sw",
                table: "Notifications",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddColumn<short>(
                name: "TriggeringStatus",
                schema: "sw",
                table: "NotificationAlerts",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Users_UserId",
                schema: "sw",
                table: "Notifications",
                column: "UserId",
                principalSchema: "sw",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Users_UserId",
                schema: "sw",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "TriggeringStatus",
                schema: "sw",
                table: "NotificationAlerts");

            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                schema: "sw",
                table: "Notifications",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Users_UserId",
                schema: "sw",
                table: "Notifications",
                column: "UserId",
                principalSchema: "sw",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}
