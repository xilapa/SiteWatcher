using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infra.Persistence.Migrations
{
    /// <inheritdoc />
    [SuppressMessage("Performance", "CA1861:Avoid constant arrays as arguments")]
    public partial class use_triggeringdate_on_notificationalerts_pk : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_NotificationAlerts",
                schema: "sw",
                table: "NotificationAlerts");

            migrationBuilder.AddPrimaryKey(
                name: "PK_NotificationAlerts",
                schema: "sw",
                table: "NotificationAlerts",
                columns: new[] { "NotificationId", "AlertId", "TriggeringDate" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_NotificationAlerts",
                schema: "sw",
                table: "NotificationAlerts");

            migrationBuilder.AddPrimaryKey(
                name: "PK_NotificationAlerts",
                schema: "sw",
                table: "NotificationAlerts",
                columns: new[] { "NotificationId", "AlertId" });
        }
    }
}
