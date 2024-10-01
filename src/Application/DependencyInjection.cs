using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Scrutor;

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

    public static IServiceCollection AddHandlers(this IServiceCollection services)
    {
        services.Scan(scan =>
        {
            scan.FromAssemblyOf<ThisAssembly>()
                .AddClasses(c => c.AssignableTo(typeof(IConsumer<>)))
                .UsingRegistrationStrategy(RegistrationStrategy.Skip)
                .AsImplementedInterfaces()
                .WithScopedLifetime();
        });
        return services;
    }
}

public sealed class ThisAssembly{}