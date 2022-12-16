using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Infra;
using SiteWatcher.Infra.Messaging;
using SiteWatcher.Worker;
using SiteWatcher.Worker.Consumers;
using SiteWatcher.Worker.Jobs;
using SiteWatcher.Worker.Persistence;
using SiteWatcher.Worker.Utils;

var host = new HostBuilder()
    .ConfigureDefaults(args)
    .ConfigureAppConfiguration((hostContext, configBuilder) =>
        configBuilder
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", true, true)
            .AddJsonFile($"appsettings.{hostContext.HostingEnvironment.EnvironmentName}.json", true, true)
            .AddEnvironmentVariables()
            .AddEnvironmentVariables("DOTNET_")
            .AddCommandLine(args)
    )
    .ConfigureWebHostDefaults(webBuilder =>
    {
        webBuilder.ConfigureServices(services =>
        {
            var configuration = services
                .BuildServiceProvider()
                .GetRequiredService<IConfiguration>();

            services.ConfigureHealthChecks(configuration);
        });

        webBuilder.Configure((context, app) =>
        {
            app.UseRouting();
            app.UseEndpoints(e => e.ConfigureHealthCheckEndpoints(context.Configuration));
        });
    })
    .ConfigureServices((hostContext, serviceCollection) =>
    {
        var workerSettings = hostContext.Configuration.Get<WorkerSettings>();
        var rabbitMqSettings = hostContext.Configuration.Get<RabbitMqSettings>();
        var emailSettings = hostContext.Configuration.Get<EmailSettings>();

        var appSettings = new WorkerAppSettings
        {
            IsDevelopment = hostContext.HostingEnvironment.IsDevelopment(),
            ConnectionString = workerSettings!.DbConnectionString,
            InMemoryStorageAndQueue = workerSettings.UseInMemoryStorageAndQueue,
            EmailNotificationRoutingKey = RoutingKeys.EmailNotification
        };

        serviceCollection
            .AddSingleton<IAppSettings>(appSettings)
            .Configure<WorkerSettings>(hostContext.Configuration)
            .SetupPersistence(appSettings)
            .SetupJobs(workerSettings)
            .SetupMessaging(rabbitMqSettings!, appSettings)
            .AddConsumers()
            .SetupEmail(emailSettings!);
    })
    .Build();

await host.RunAsync();