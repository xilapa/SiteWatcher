using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SiteWatcher.Infra.HealthChecks;

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
                new PostgresConnectionHealthCheck(workerSettings.DbConnectionString),
                tags: new[] { HealthCheckSettings.TagDatabase }
            )
            .AddCheck("EmailHost",
                 new EmailHealthCheck(emailSettings),
                tags: new[] { HealthCheckSettings.TagEmailHost }
            )
            .AddCheck("RabbitMq",
                new RabbitMqHealthCheck(rabbitMqSettins),
                tags: new[] { HealthCheckSettings.TagRabbitMq });

        services.AddHealthChecks();

        return services;
    }

    public static IEndpointRouteBuilder ConfigureHealthCheckEndpoints(this IEndpointRouteBuilder endpointBuilder, IConfiguration configuration)
    {
        var healthCheckSettings = configuration.Get<HealthCheckSettings>();

        // Simple connect endpoint
        endpointBuilder.MapHealthChecks(healthCheckSettings.BasePath,
        new HealthCheckOptions
        {
            Predicate = hc => hc.Tags.Count == 0,
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });

        // Full dependencies check endpoint
        endpointBuilder.MapHealthChecks(healthCheckSettings.FullCheckPath,
        new HealthCheckOptions
        {
            Predicate = hc => hc.Tags.Count != 0,
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });

        endpointBuilder.MapHealthChecksUI(opts => opts.UIPath = healthCheckSettings.UiPath);
        return endpointBuilder;
    }

    // TODO: remove UI and use grafana with prometheus
    public static IServiceCollection ConfigureHealthChecksUI(this IServiceCollection services, IConfiguration configuration)
    {
        var workerSettings = configuration.Get<WorkerSettings>();
        var healthCheckSettings = configuration.Get<HealthCheckSettings>();

        services.AddHealthChecksUI(opts =>
        {
            opts.SetApiMaxActiveRequests(1);
            opts.MaximumHistoryEntriesPerEndpoint(100);
            opts.AddHealthCheckEndpoint(
                "Worker - Simple Connect",
                $"{healthCheckSettings.BaseHost}/{healthCheckSettings.BasePath}"
            );
            opts.AddHealthCheckEndpoint(
                "Worker - Full dependency chek",
                $"{healthCheckSettings.BaseHost}/{healthCheckSettings.FullCheckPath}"
            );
            opts.SetEvaluationTimeInSeconds(15 * 60);
        })
        .AddPostgreSqlStorage(workerSettings.DbConnectionString);

        return services;
    }
}