namespace SiteWatcher.WebAPI.Extensions;

public static class StartupExtensions 
{
    public static WebApplicationBuilder UseStartup<T>(this WebApplicationBuilder builder) where T : IStartup
    {
        if(Activator.CreateInstance(typeof(T), builder.Configuration) is not IStartup startup)
            throw new NullReferenceException(nameof(startup));

        startup.ConfigureServices(builder.Services, builder.Environment);

        var app = builder.Build();
        var loggerFactory = app.Services.GetService<ILoggerFactory>();
        startup.Configure(app, app.Environment, loggerFactory!);
        app.Run();

        return builder;
    }
}