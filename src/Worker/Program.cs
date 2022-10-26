using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SiteWatcher.Worker;
using SiteWatcher.Worker.Jobs;

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
        serviceCollection
            .Configure<WorkerSettings>(hostContext.Configuration.GetSection(nameof(WorkerSettings)));

        var workerSettings = hostContext.Configuration
            .GetSection(nameof(WorkerSettings)).Get<WorkerSettings>();

        serviceCollection.AddJobs(workerSettings);
    })
    .Build();

await host.RunAsync();