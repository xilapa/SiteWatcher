using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infra.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class populate_notificationalerts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
-- SET UserId on Notification
UPDATE ""sw"".""Notifications"" n
SET ""UserId"" = e.""UserId""
FROM ""sw"".""Emails"" e
    WHERE n.""EmailId"" = e.""Id"";

-- Populate Notification Alerts
INSERT INTO ""sw"".""NotificationAlerts""(""NotificationId"",""AlertId"",""TriggeringDate"")
SELECT ""Id"",""AlertId"", ""CreatedAt"" FROM ""sw"".""Notifications"";");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        { }
    }
}
