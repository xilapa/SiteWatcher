using Microsoft.Extensions.DependencyInjection;
using SiteWatcher.Application.Users.Commands.RegisterUser;

namespace SiteWatcher.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg=>cfg.RegisterServicesFromAssemblies(typeof(RegisterUserCommand).Assembly));
        // ExecuteAlertsCommand is added only on worker
        return services;
    }
}