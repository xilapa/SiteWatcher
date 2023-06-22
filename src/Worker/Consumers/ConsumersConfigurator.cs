using Infra.Persistence.Repositories;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using SiteWatcher.Application.Emails.EventHandlers;
using SiteWatcher.Application.Notifications.Commands.ProcessNotifications;
using SiteWatcher.Domain.Emails.Events;
using SiteWatcher.Domain.Notifications.Repositories;

namespace SiteWatcher.Worker.Consumers;

public static class ConsumersConfigurator
{
    public static IServiceCollection AddConsumers(this IServiceCollection serviceCollection)
    {
        serviceCollection
            .AddTransient<IEmailNotificationConsumer, EmailNotificationConsumer>()
            .AddTransient<AlertsTriggeredEventConsumer>()
            .AddScoped<ProcessNotificationCommandHandler>()
            .AddScoped<INotificationRepository, NotificationRepository>()
            .AddScoped<INotificationHandler<EmailCreatedEvent>,EmailCreatedEventHandler>();
        return serviceCollection;
    }
}