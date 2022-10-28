using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infra.Persistence.Migrations
{
    public partial class idempotent_consumers : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "worker");

            migrationBuilder.CreateTable(
                name: "IdempotentConsumers",
                schema: "worker",
                columns: table => new
                {
                    MessageId = table.Column<string>(type: "varchar(128)", nullable: false),
                    Consumer = table.Column<string>(type: "varchar(128)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IdempotentConsumers", x => new { x.MessageId, x.Consumer });
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IdempotentConsumers",
                schema: "worker");
        }
    }
}
