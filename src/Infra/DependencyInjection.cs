using Dapper;
using DotNetCore.CAP.Internal;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Savorboard.CAP.InMemoryMessageQueue;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Common.Repositories;
using SiteWatcher.Common.Services;
using SiteWatcher.Domain.Alerts.Repositories;
using SiteWatcher.Domain.Authentication;
using SiteWatcher.Domain.Common.Services;
using SiteWatcher.Domain.Common.ValueObjects;
using SiteWatcher.Domain.Users.Repositories;
using SiteWatcher.Infra.Authorization;
using SiteWatcher.Infra.Cache;
using SiteWatcher.Infra.DapperRepositories;
using SiteWatcher.Infra.EmailSending;
using SiteWatcher.Infra.FireAndForget;
using SiteWatcher.Infra.Messaging;
using SiteWatcher.Infra.Repositories;
using StackExchange.Redis;

namespace SiteWatcher.Infra;

public static class DependencyInjection
{
    public static IServiceCollection AddDataContext<TContext>(this IServiceCollection services) where TContext : DbContext, IUnitOfWork
    {
        // Making explicit that the context is the same for all repositories
        services.AddDbContext<TContext>(ServiceLifetime.Scoped);
        services.AddScoped<IUnitOfWork>(s => s.GetRequiredService<TContext>());

        // Add migrator
        services.AddScoped(typeof(DatabaseMigrator));

        return services;
    }

    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IAlertRepository, AlertRepository>();
        return services;
    }

    public static IServiceCollection AddDapperRepositories(this IServiceCollection services)
    {
        services.AddSingleton<IDapperQueries, DapperQueries>();
        services.AddScoped<IDapperContext, DapperContext>();
        services.AddScoped<IUserDapperRepository, UserDapperRepository>();
        services.AddScoped<IAlertDapperRepository, AlertDapperRepository>();
        SqlMapper.AddTypeHandler(new UserId.DapperTypeHandler());
        return services;
    }

    public static IServiceCollection AddRedisCache(this IServiceCollection services, IAppSettings appSettings)
    {
        var configOptions = ConfigurationOptions.Parse(appSettings.RedisConnectionString);
        configOptions.AbortOnConnectFail = true;
        configOptions.ConnectRetry = 3;
        configOptions.ConnectTimeout = 2_000;
        configOptions.ReconnectRetryPolicy = new ExponentialRetry(TimeSpan.FromSeconds(5).Milliseconds, TimeSpan.FromSeconds(20).Milliseconds);

        services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(configOptions));
        services.AddSingleton<ICache, RedisCache>();
        return services;
    }

    public static IServiceCollection AddSession(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ISession, Session>();
        return services;
    }

    public static IServiceCollection AddEmailService(this IServiceCollection services)
    {
        services.AddScoped<IEmailService, EmailService>();
        return services;
    }

    public static IServiceCollection AddFireAndForgetService(this IServiceCollection services)
    {
        services.AddScoped<IFireAndForgetService, FireAndForgetService>();
        return services;
    }

    public static IServiceCollection AddIdHasher(this IServiceCollection services)
    {
        services.AddSingleton<IIdHasher, IdHasher.IdHasher>();
        return services;
    }

    public static IServiceCollection SetupMessaging(this IServiceCollection services, RabbitMqSettings rabbitSettings, IAppSettings appSettings)
    {
        services
            .AddCap(opts =>
            {
                if (appSettings.InMemoryStorageAndQueue)
                {
                    opts.UseInMemoryStorage();
                    opts.UseInMemoryMessageQueue();
                }
                else
                {
                    CreateExchange(rabbitSettings);
                    opts.UseEntityFramework<SiteWatcherContext>();
                    opts.UseRabbitMQ(opt =>
                    {
                        opt.HostName = rabbitSettings.Host;
                        opt.VirtualHost = rabbitSettings.VirtualHost;
                        opt.UserName = rabbitSettings.UserName;
                        opt.Password = rabbitSettings.Password;
                        opt.Port = rabbitSettings.Port;

                        // Configuration to publish/consume messages from a custom exchange
                        opt.ExchangeName = RabbitMqSettings.SiteWatcherExchange;
                        opt.CustomHeaders = message => new List<KeyValuePair<string, string>>
                        {
                            new KeyValuePair<string, string>(DotNetCore.CAP.Messages.Headers.MessageId, GetMessageId(message, appSettings.MessageIdKey)),
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

        services.AddScoped<IPublisher, Publisher>();
        services.AddScoped<IPublishService, PublishService>();

        return services;
    }

    private static string GetMessageId(BasicDeliverEventArgs message, string headerKey)
    {
        var hasMessageId = message.BasicProperties.Headers.TryGetValue(headerKey, out var messageId);
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
            RabbitMqSettings.SiteWatcherExchange,
            ExchangeType.Topic,
            durable: true,
            autoDelete: false);
    }

    public static IServiceCollection SetupDataProtection(this IServiceCollection services, IAppSettings appSettings)
    {
        if (appSettings.DisableDataProtectionRedisStore)
        {
            services.AddDataProtection();
            return services;
        }

        var redisMultiplexer = services.BuildServiceProvider().GetRequiredService<IConnectionMultiplexer>();
        services
            .AddDataProtection()
            .PersistKeysToStackExchangeRedis(redisMultiplexer);

        return services;
    }
}