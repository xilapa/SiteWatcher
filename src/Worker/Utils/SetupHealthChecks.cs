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

        endpointBuilder.MapHealthChecks(healthCheckSettings.BasePath,
        new HealthCheckOptions
        {
            Predicate = hc => hc.Tags.Count == 0,
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });

        endpointBuilder.MapHealthChecks(healthCheckSettings.DbPath,
        new HealthCheckOptions
        {
            Predicate = hc => hc.Tags.Contains(HealthCheckSettings.TagDatabase),
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });

        endpointBuilder.MapHealthChecks(healthCheckSettings.EmailHostPath,
        new HealthCheckOptions
        {
            Predicate = hc => hc.Tags.Contains(HealthCheckSettings.TagEmailHost),
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });

        endpointBuilder.MapHealthChecks(healthCheckSettings.RabbitMqPath,
        new HealthCheckOptions
        {
            Predicate = hc => hc.Tags.Contains(HealthCheckSettings.TagRabbitMq),
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
                "Worker",
                $"{healthCheckSettings.BaseHost}/{healthCheckSettings.BasePath}"
            );
            opts.AddHealthCheckEndpoint(
                "Database",
                $"{healthCheckSettings.BaseHost}/{healthCheckSettings.DbPath}"
            );
            opts.AddHealthCheckEndpoint(
                "EmailHost",
                $"{healthCheckSettings.BaseHost}/{healthCheckSettings.EmailHostPath}"
            );
            opts.AddHealthCheckEndpoint(
                "RabbitMq",
                $"{healthCheckSettings.BaseHost}/{healthCheckSettings.RabbitMqPath}"
            );
            opts.SetEvaluationTimeInSeconds(15 * 60);
        })
        .AddPostgreSqlStorage(workerSettings.DbConnectionString);

        return services;
    }
}