using System;
using System.Diagnostics.CodeAnalysis;
using SiteWatcher.WebAPI.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace SiteWatcher.WebAPI.Extensions;

public static class StartupExtensions 
{
    [SuppressMessage("Suggestion", "IDE0019: Use pattern matching", Justification = "Não quero usar.")]
    public static WebApplicationBuilder UseStartup<T>(this WebApplicationBuilder builder) where T : IStartup
    {
        var startup = Activator.CreateInstance(typeof(T), builder.Configuration) as IStartup;
        if(startup is null) throw new NullReferenceException(nameof(startup));
        
        startup.ConfigureServices(builder.Services, builder.Environment);

        var app = builder.Build();
        var loggerFactory = app.Services.GetService<ILoggerFactory>();
        startup.Configure(app, app.Environment, loggerFactory);
        app.Run();

        return builder;
    }
}