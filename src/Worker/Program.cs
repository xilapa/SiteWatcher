using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

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
            .AddJsonFile($"appsettings.{hostContext.HostingEnvironment}.json", true, true)
            .AddEnvironmentVariables("DOTNET_")
            .AddCommandLine(args)
    )
    // .ConfigureServices()
    .Build();

await host.RunAsync();