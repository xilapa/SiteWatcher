using DotNetCore.CAP.Internal;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using Savorboard.CAP.InMemoryMessageQueue;
using SiteWatcher.Infra;

namespace SiteWatcher.Worker.Messaging;

public static class MessagingConfigurator
{
    public static IServiceCollection SetupMessaging(this IServiceCollection serviceCollection, WorkerSettings settings)
    {
        CreateExchange(settings);

        serviceCollection
            .AddCap(opts =>
            {
                if (settings.UseInMemoryStorageAndQueue)
                {
                    opts.UseInMemoryStorage();
                    opts.UseInMemoryMessageQueue();
                }
                else
                {
                    opts.UseEntityFramework<SiteWatcherContext>();
                    opts.UseRabbitMQ(opt =>
                    {
                        opt.HostName = settings.RabbitMq.Host;
                        opt.UserName = settings.RabbitMq.UserName;
                        opt.Password = settings.RabbitMq.Password;
                        opt.Port = settings.RabbitMq.Port;

                        // Configuration to publish/consume messages from a custom exchange
                        opt.ExchangeName = Exchanges.SiteWatcher;
                        opt.CustomHeaders = message => new List<KeyValuePair<string, string>>
                        {
                            new KeyValuePair<string, string>(DotNetCore.CAP.Messages.Headers.MessageId, SnowflakeId.Default().NextId().ToString()),
                            new KeyValuePair<string, string>(DotNetCore.CAP.Messages.Headers.MessageName, message.RoutingKey)
                        };
                    });
                }

                opts.FailedRetryCount = 3;
                opts.ConsumerThreadCount = settings.Consumers.PerQueueConcurrency == 0 ? Environment.ProcessorCount : settings.Consumers.PerQueueConcurrency;
                // Enable the concurrency level to be per queue
                opts.UseDispatchingPerGroup = true;
            });
        return serviceCollection;
    }

    private static void CreateExchange(WorkerSettings settings)
    {
        var connectionFactory = new ConnectionFactory
        {
            HostName = settings.RabbitMq.Host,
            UserName = settings.RabbitMq.UserName,
            Password = settings.RabbitMq.Password
        };

        using var connection = connectionFactory.CreateConnection();
        using var channel = connection.CreateModel();

        // Create a topic exchange for site watcher
        channel.ExchangeDeclare(
            Exchanges.SiteWatcher,
            ExchangeType.Topic,
            durable: true,
            autoDelete: false);
    }
}