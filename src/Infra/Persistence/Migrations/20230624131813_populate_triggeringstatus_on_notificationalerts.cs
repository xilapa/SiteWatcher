using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infra.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class populate_triggeringstatus_on_notificationalerts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                UPDATE ""sw"".""NotificationAlerts"" na
                SET ""TriggeringStatus"" = t.""Status""
                FROM ""sw"".""Triggerings"" t
                    WHERE 
                na.""AlertId"" = t.""AlertId""
                AND na.""TriggeringDate"" = t.""Date"";");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
