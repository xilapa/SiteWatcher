using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infra.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class populate_triggerings_with_notifications_data : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
            INSERT INTO ""sw"".""Triggerings""(""Date"", ""AlertId"")
            SELECT ""CreatedAt"", ""AlertId"" FROM ""sw"".""Notifications"";");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DELETE FROM ""sw"".""Triggerings"";");
        }
    }
}
