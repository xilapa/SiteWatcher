using Microsoft.Extensions.DependencyInjection;
using Savorboard.CAP.InMemoryMessageQueue;
using SiteWatcher.Infra;

namespace SiteWatcher.Worker.Messaging;

public static class MessagingConfigurator
{
    public static IServiceCollection SetupMessaging(this IServiceCollection serviceCollection, WorkerSettings settings)
    {
        // TODO: create the exchanges and bind them
        serviceCollection
            .AddCap(opts => {
                if (settings.UseInMemoryStorageAndQueue)
                {
                    opts.UseInMemoryStorage();
                    opts.UseInMemoryMessageQueue();
                }
                else
                {
                    opts.UseEntityFramework<SiteWatcherContext>();
                    opts.UseRabbitMQ(opt => {
                        opt.HostName = settings.RabbitMq.Host;
                        opt.UserName = settings.RabbitMq.UserName;
                        opt.Password = settings.RabbitMq.Password;
                        opt.Port = settings.RabbitMq.Port;
                    });
                }

                opts.FailedRetryCount = 3;
            });
        return serviceCollection;
    }
}