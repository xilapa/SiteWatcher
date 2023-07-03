using Microsoft.Extensions.DependencyInjection;

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
}