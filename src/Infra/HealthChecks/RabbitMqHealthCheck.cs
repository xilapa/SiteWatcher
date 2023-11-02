using Microsoft.Extensions.Diagnostics.HealthChecks;
using RabbitMQ.Client;
using SiteWatcher.Infra.Messaging;

namespace SiteWatcher.Infra.HealthChecks;

public sealed class RabbitMqHealthCheck : IHealthCheck
{
    private readonly RabbitMqSettings _settings;

    public RabbitMqHealthCheck(RabbitMqSettings settings)
    {
        _settings = settings;
    }

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var connectionFactory = new ConnectionFactory
        {
            HostName = _settings.Host,
            VirtualHost = _settings.VirtualHost,
            UserName = _settings.UserName,
            Password = _settings.Password,
            Port = _settings.Port
        };

        try
        {
            var fiveSecondsToken = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            using var connection = connectionFactory.CreateConnection();
            using var channel = connection.CreateModel();
            fiveSecondsToken.Token.ThrowIfCancellationRequested();
        }
        catch (Exception e)
        {
            if (e is OperationCanceledException || e is TaskCanceledException || e is TimeoutException)
                return Task.FromResult(HealthCheckResult.Degraded("Took more than five seconds to connect to rabbitmq", e));

            return Task.FromResult(HealthCheckResult.Unhealthy("Cannot connect the rabbitmq", e));
        }

        return Task.FromResult(HealthCheckResult.Healthy());
    }
}