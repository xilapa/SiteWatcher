using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infra.Persistence.Migrations
{
    public partial class email_id_guid : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "EmailId",
                schema: "siteWatcher_webApi",
                table: "Notifications",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                schema: "siteWatcher_webApi",
                table: "Emails",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "uuid");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "EmailId",
                schema: "siteWatcher_webApi",
                table: "Notifications",
                type: "int",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                schema: "siteWatcher_webApi",
                table: "Emails",
                type: "int",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);
        }
    }
}
