using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infra.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class populate_triggering_status : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
---- Success triggerings
--en-us
INSERT INTO ""sw"".""Triggerings""(""Date"", ""AlertId"", ""Status"")
SELECT
n.""CreatedAt"",
n.""AlertId"",
1 ""Status"" -- Success
    FROM ""sw"".""Notifications"" n 
    INNER JOIN ""sw"".""Emails"" e ON e.""Id"" = n.""EmailId""
INNER JOIN ""sw"".""Alerts"" a ON a.""Id"" = n.""AlertId""
WHERE
e.""Body"" LIKE concat('%have been triggered%',a.""Name"",'%')
AND
e.""Body"" NOT LIKE concat('%couldn''t be reached%', a.""Name"", '%');

--es-es
INSERT INTO ""sw"".""Triggerings""(""Date"", ""AlertId"", ""Status"")
SELECT
n.""CreatedAt"",
n.""AlertId"",
1 ""Status"" -- Success
    FROM ""sw"".""Notifications"" n 
    INNER JOIN ""sw"".""Emails"" e ON e.""Id"" = n.""EmailId""
INNER JOIN ""sw"".""Alerts"" a ON a.""Id"" = n.""AlertId""
WHERE
e.""Body"" LIKE concat('%Se han activado las siguientes alertas%',a.""Name"",'%')
AND
e.""Body"" NOT LIKE concat('%No se pudo acceder a los siguientes sitios de alertas%', a.""Name"", '%');

--pt-br
INSERT INTO ""sw"".""Triggerings""(""Date"", ""AlertId"", ""Status"")
SELECT
n.""CreatedAt"",
n.""AlertId"",
1 ""Status"" -- Success
    FROM ""sw"".""Notifications"" n 
    INNER JOIN ""sw"".""Emails"" e ON e.""Id"" = n.""EmailId""
INNER JOIN ""sw"".""Alerts"" a ON a.""Id"" = n.""AlertId""
WHERE
e.""Body"" LIKE concat('%foram disparados%',a.""Name"",'%')
AND
e.""Body"" NOT LIKE concat('%Não foi possível acessar os seguintes sites de alertas%', a.""Name"", '%');


---- Error triggerings
--en-us
INSERT INTO ""sw"".""Triggerings""(""Date"", ""AlertId"", ""Status"")
SELECT
n.""CreatedAt"",
n.""AlertId"",
2 ""Status"" -- Success
    FROM ""sw"".""Notifications"" n 
    INNER JOIN ""sw"".""Emails"" e ON e.""Id"" = n.""EmailId""
INNER JOIN ""sw"".""Alerts"" a ON a.""Id"" = n.""AlertId""
WHERE
e.""Body"" NOT LIKE concat('%have been triggered%',a.""Name"",'%')
AND
e.""Body"" LIKE concat('%couldn''t be reached%', a.""Name"", '%');

--es-es
INSERT INTO ""sw"".""Triggerings""(""Date"", ""AlertId"", ""Status"")
SELECT
n.""CreatedAt"",
n.""AlertId"",
2 ""Status"" -- Success
    FROM ""sw"".""Notifications"" n 
    INNER JOIN ""sw"".""Emails"" e ON e.""Id"" = n.""EmailId""
INNER JOIN ""sw"".""Alerts"" a ON a.""Id"" = n.""AlertId""
WHERE
e.""Body"" NOT LIKE concat('%Se han activado las siguientes alertas%',a.""Name"",'%')
AND
e.""Body"" LIKE concat('%No se pudo acceder a los siguientes sitios de alertas%', a.""Name"", '%');

--pt-br
INSERT INTO ""sw"".""Triggerings""(""Date"", ""AlertId"", ""Status"")
SELECT
n.""CreatedAt"",
n.""AlertId"",
2 ""Status"" -- Success
    FROM ""sw"".""Notifications"" n 
    INNER JOIN ""sw"".""Emails"" e ON e.""Id"" = n.""EmailId""
INNER JOIN ""sw"".""Alerts"" a ON a.""Id"" = n.""AlertId""
WHERE
e.""Body"" NOT LIKE concat('%foram disparados%',a.""Name"",'%')
AND
e.""Body"" LIKE concat('%Não foi possível acessar os seguintes sites de alertas%', a.""Name"", '%');
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        { }
    }
}
