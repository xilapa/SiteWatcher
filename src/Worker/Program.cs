using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SiteWatcher.Worker;
using SiteWatcher.Worker.Consumers;
using SiteWatcher.Worker.Jobs;
using SiteWatcher.Worker.Messaging;
using SiteWatcher.Worker.Persistence;
using SiteWatcher.Worker.Utils;

var host = new HostBuilder()
    .ConfigureDefaults(args)
    .ConfigureAppConfiguration((hostContext, configBuilder) =>
        configBuilder
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", true, true)
            .AddJsonFile($"appsettings.{hostContext.HostingEnvironment.EnvironmentName}.json", true, true)
            .AddEnvironmentVariables("DOTNET_")
            .AddCommandLine(args)
    )
    .ConfigureServices((hostContext, serviceCollection)=> {
        var workerSettings = hostContext.Configuration.Get<WorkerSettings>();
        var rabbitMqSettings = hostContext.Configuration.Get<RabbitMqSettings>();
        var emailSettings = hostContext.Configuration.Get<EmailSettings>();

        serviceCollection
            .Configure<WorkerSettings>(hostContext.Configuration)
            .SetupPersistence(workerSettings, hostContext.HostingEnvironment)
            .SetupJobs(workerSettings, hostContext.HostingEnvironment)
            .SetupMessaging(workerSettings, rabbitMqSettings)
            .SetupConsumers()
            .AddHttpClient()
            .SetupEmail(emailSettings);
    })
    .Build();

await host.RunAsync();