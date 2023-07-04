using Microsoft.Extensions.DependencyInjection;
using Scrutor;
using SiteWatcher.Application.Common.Messages;

namespace SiteWatcher.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediator(opts =>
        {
            opts.Namespace = "SiteWatcher.Application.Mediator";
            opts.ServiceLifetime = ServiceLifetime.Scoped;
        });
        return services;
    }

    public static IServiceCollection AddMessageHandlers(this IServiceCollection services)
    {
        services.Scan(scan =>
        {
            scan.FromAssemblyOf<ThisAssembly>()
                .AddClasses(c => c.AssignableTo(typeof(IMessageHandler<>)))
                .UsingRegistrationStrategy(RegistrationStrategy.Skip)
                .AsImplementedInterfaces()
                .WithScopedLifetime();
        });
        return services;
    }
}

public sealed class ThisAssembly{}