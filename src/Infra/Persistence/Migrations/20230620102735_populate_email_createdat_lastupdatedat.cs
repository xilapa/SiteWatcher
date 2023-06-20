using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infra.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class populate_email_createdat_lastupdatedat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
UPDATE ""sw"".""Emails"" e
            SET 
            ""CreatedAt"" = sub.""CreatedAt"",
            ""LastUpdatedAt"" = sub.""CreatedAt""
            FROM (
                SELECT n.""EmailId"", n.""CreatedAt""
            FROM ""sw"".""Notifications"" n
                WHERE ""EmailId"" IS NOT NULL
                ) sub
                WHERE
            e.""Id"" = sub.""EmailId""
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        { }
    }
}
