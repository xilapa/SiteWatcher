using Microsoft.Extensions.DependencyInjection;

namespace SiteWatcher.Worker.Consumers;

public static class ConsumersConfigurator
{
    public static IServiceCollection SetupConsumers(this IServiceCollection serviceCollection)
    {
        serviceCollection
            .AddTransient<IEmailNotificationConsumer, EmailNotificationConsumer>();
        return serviceCollection;
    }
}