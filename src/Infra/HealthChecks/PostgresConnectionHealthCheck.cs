using Microsoft.Extensions.Diagnostics.HealthChecks;
using Npgsql;
namespace SiteWatcher.Infra.HealthChecks;

public sealed class PostgresConnectionHealthCheck : IHealthCheck
{
    private const string _testQuery = "SELECT 1";
    private readonly string _connectionString;

    public PostgresConnectionHealthCheck(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var fiveSecondsToken = new CancellationTokenSource(TimeSpan.FromSeconds(5));

        using var connection = new NpgsqlConnection(_connectionString);

        try
        {
            if (connection.State == System.Data.ConnectionState.Closed)
                await connection.OpenAsync(fiveSecondsToken.Token);

            var command = connection.CreateCommand();
            command.CommandText = _testQuery;
            await command.ExecuteNonQueryAsync(fiveSecondsToken.Token);
        }
        catch(Exception e)
        {
            if (e is OperationCanceledException || e is TaskCanceledException || e is TimeoutException)
                return HealthCheckResult.Degraded("Took more than five seconds to query the database", e);

            return HealthCheckResult.Unhealthy("Cannot connect to the database", e);
        }

        return HealthCheckResult.Healthy();
    }
}