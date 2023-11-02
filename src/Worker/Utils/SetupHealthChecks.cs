using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SiteWatcher.Infra.EmailSending;
using SiteWatcher.Infra.HealthChecks;
using SiteWatcher.Infra.Messaging;

namespace SiteWatcher.Worker.Utils;

public static class SetupHealthChecks
{
    public static IServiceCollection ConfigureHealthChecks(this IServiceCollection services, IConfiguration configuration)
    {
        var workerSettings = configuration.Get<WorkerSettings>();
        var emailSettings = configuration.Get<EmailSettings>();
        var rabbitMqSettins = configuration.Get<RabbitMqSettings>();

        services.AddHealthChecks()
            .AddCheck(
                "Database",
                new PostgresConnectionHealthCheck(workerSettings!.DbConnectionString),
                tags: new[] { HealthCheckSettings.TagDatabase }
            )
            .AddCheck("EmailHost",
                 new EmailHealthCheck(emailSettings!),
                tags: new[] { HealthCheckSettings.TagEmailHost }
            )
            .AddCheck("RabbitMq",
                new RabbitMqHealthCheck(rabbitMqSettins!),
                tags: new[] { HealthCheckSettings.TagRabbitMq });

        services.AddHealthChecks();

        return services;
    }

    public static IEndpointRouteBuilder ConfigureHealthCheckEndpoints(this IEndpointRouteBuilder endpointBuilder, IConfiguration configuration)
    {
        var healthCheckSettings = configuration.Get<HealthCheckSettings>();

        // Simple connect endpoint
        endpointBuilder.MapHealthChecks(healthCheckSettings!.BasePath,
        new HealthCheckOptions
        {
            Predicate = hc => hc.Tags.Count == 0,
        });

        // Full dependencies check endpoint
        endpointBuilder.MapHealthChecks(healthCheckSettings.FullCheckPath,
        new HealthCheckOptions
        {
            Predicate = hc => hc.Tags.Count != 0,
        });

        return endpointBuilder;
    }
}