using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SiteWatcher.Worker;
using SiteWatcher.Worker.Jobs;
using SiteWatcher.Worker.Messaging;
using SiteWatcher.Worker.Persistence;

var host = new HostBuilder()
    .ConfigureDefaults(args)
    .ConfigureHostConfiguration(configBuilder =>
        configBuilder
            .SetBasePath(AppContext.BaseDirectory)
            .AddEnvironmentVariables("DOTNET_")
            .AddCommandLine(args)
    )
    .ConfigureAppConfiguration((hostContext, configBuilder) =>
        configBuilder
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", true, true)
            .AddJsonFile($"appsettings.{hostContext.HostingEnvironment.EnvironmentName}.json", true, true)
            .AddEnvironmentVariables("DOTNET_")
            .AddCommandLine(args)
    )
    .ConfigureServices((hostContext, serviceCollection)=> {
        var workerSettings = new WorkerSettings(hostContext.HostingEnvironment);
        hostContext.Configuration
            .GetSection(nameof(WorkerSettings)).Bind(workerSettings);

        serviceCollection
            .Configure<WorkerSettings>(hostContext.Configuration.GetSection(nameof(WorkerSettings)))
            .SetupPersistence(workerSettings)
            .SetupJobs(workerSettings)
            .SetupMessaging(workerSettings);
    })
    .Build();

await host.RunAsync();