using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infra.Persistence.Migrations
{
    public partial class email_and_notification_relationship : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                schema: "siteWatcher_webApi",
                table: "Notifications",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<Guid>(
                name: "EmailId",
                schema: "siteWatcher_webApi",
                table: "Notifications",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Emails",
                schema: "siteWatcher_webApi",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Recipients = table.Column<string>(type: "text", nullable: false),
                    DateSent = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    Subject = table.Column<string>(type: "varchar(255)", nullable: false),
                    Body = table.Column<string>(type: "text", nullable: false),
                    ErrorMessage = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Emails", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_EmailId",
                schema: "siteWatcher_webApi",
                table: "Notifications",
                column: "EmailId");

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Emails_EmailId",
                schema: "siteWatcher_webApi",
                table: "Notifications",
                column: "EmailId",
                principalSchema: "siteWatcher_webApi",
                principalTable: "Emails",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Emails_EmailId",
                schema: "siteWatcher_webApi",
                table: "Notifications");

            migrationBuilder.DropTable(
                name: "Emails",
                schema: "siteWatcher_webApi");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_EmailId",
                schema: "siteWatcher_webApi",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "EmailId",
                schema: "siteWatcher_webApi",
                table: "Notifications");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                schema: "siteWatcher_webApi",
                table: "Notifications",
                type: "integer",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");
        }
    }
}
