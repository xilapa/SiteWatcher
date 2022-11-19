using DotNetCore.CAP.Internal;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Savorboard.CAP.InMemoryMessageQueue;
using SiteWatcher.Infra;

namespace SiteWatcher.Worker.Messaging;

public static class MessagingConfigurator
{
    public static IServiceCollection SetupMessaging(this IServiceCollection serviceCollection, WorkerSettings settings, RabbitMqSettings rabbitSettings)
    {
        CreateExchange(rabbitSettings);

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
                        opt.HostName = rabbitSettings.Host;
                        opt.VirtualHost = rabbitSettings.Host;
                        opt.UserName = rabbitSettings.UserName;
                        opt.Password = rabbitSettings.Password;
                        opt.Port = rabbitSettings.Port;

                        // Configuration to publish/consume messages from a custom exchange
                        opt.ExchangeName = Exchanges.SiteWatcher;
                        opt.CustomHeaders = message => new List<KeyValuePair<string, string>>
                        {
                            new KeyValuePair<string, string>(DotNetCore.CAP.Messages.Headers.MessageId, GetMessageId(message)),
                            new KeyValuePair<string, string>(DotNetCore.CAP.Messages.Headers.MessageName, message.RoutingKey)
                        };
                    });
                }

                opts.FailedRetryCount = 25;
                // Email notification consumer needs to have only one consumer
                opts.ConsumerThreadCount = 1;
                // Enable the concurrency level to be per queue
                opts.UseDispatchingPerGroup = true;
            });
        return serviceCollection;
    }

    private static string GetMessageId(BasicDeliverEventArgs message)
    {
        var hasMessageId = message.BasicProperties.Headers.TryGetValue(MessageHeaders.MessageIdKey, out var messageId);
        if (hasMessageId && messageId != null)
        {
            var byteMessageId = (byte[])messageId;
            var stringMessageId = byteMessageId != null ? System.Text.Encoding.UTF8.GetString(byteMessageId) : string.Empty;

            return string.IsNullOrEmpty(stringMessageId) ?
                SnowflakeId.Default().NextId().ToString()
                : stringMessageId;
        }

        return SnowflakeId.Default().NextId().ToString();
    }

    private static void CreateExchange(RabbitMqSettings settings)
    {
        var connectionFactory = new ConnectionFactory
        {
            HostName = settings.Host,
            VirtualHost = settings.VirtualHost,
            UserName = settings.UserName,
            Password = settings.Password,
            Port = settings.Port
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