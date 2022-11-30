using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infra.Persistence.Migrations
{
    public partial class user_emails_relationship : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Recipients",
                schema: "siteWatcher_webApi",
                table: "Emails");

            migrationBuilder.AddColumn<string>(
                name: "Recipient",
                schema: "siteWatcher_webApi",
                table: "Emails",
                type: "varchar(385)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                schema: "siteWatcher_webApi",
                table: "Emails",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Emails_UserId",
                schema: "siteWatcher_webApi",
                table: "Emails",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Emails_Users_UserId",
                schema: "siteWatcher_webApi",
                table: "Emails",
                column: "UserId",
                principalSchema: "siteWatcher_webApi",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Emails_Users_UserId",
                schema: "siteWatcher_webApi",
                table: "Emails");

            migrationBuilder.DropIndex(
                name: "IX_Emails_UserId",
                schema: "siteWatcher_webApi",
                table: "Emails");

            migrationBuilder.DropColumn(
                name: "Recipient",
                schema: "siteWatcher_webApi",
                table: "Emails");

            migrationBuilder.DropColumn(
                name: "UserId",
                schema: "siteWatcher_webApi",
                table: "Emails");

            migrationBuilder.AddColumn<string>(
                name: "Recipients",
                schema: "siteWatcher_webApi",
                table: "Emails",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
