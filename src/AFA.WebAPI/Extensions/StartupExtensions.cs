using System;
using AFA.WebAPI.Interfaces;
using Microsoft.AspNetCore.Builder;

namespace AFA.WebAPI.Extensions;

public static class StartupExtensions 
{
    public static WebApplicationBuilder UseStartup<T>(this WebApplicationBuilder builder) where T : IStartup
    {
        var startup = Activator.CreateInstance(typeof(T), builder.Configuration) as IStartup;
        if(startup is null) throw new ArgumentNullException($"{nameof(startup)} is null");

        startup.ConfigureServices(builder.Services);

        var app = builder.Build();
        startup.Configure(app, app.Environment);
        app.Run();

        return builder;
    }
}