using System.Runtime.CompilerServices;
using Dapper;
using MailKit.Net.Smtp;
using MassTransit;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SiteWatcher.Application.Common.Queries;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Common.Services;
using SiteWatcher.Domain.Authentication;
using SiteWatcher.Domain.Authentication.Services;
using SiteWatcher.Domain.Common.Services;
using SiteWatcher.Domain.Common.ValueObjects;
using SiteWatcher.Infra.Authorization;
using SiteWatcher.Infra.Cache;
using SiteWatcher.Infra.DapperRepositories;
using SiteWatcher.Infra.EmailSending;
using SiteWatcher.Infra.Messaging;
using SiteWatcher.Infra.Persistence;
using StackExchange.Redis;

namespace SiteWatcher.Infra;

public static class DependencyInjection
{
    public static IServiceCollection AddDataContext(this IServiceCollection services, bool addMigrator = true)
    {
        services.AddDbContext<SiteWatcherContext>(ServiceLifetime.Scoped);
        services.AddScoped<ISiteWatcherContext>(sp => sp.GetRequiredService<SiteWatcherContext>());

        // Add migrator
        if (addMigrator) services.AddScoped(typeof(DatabaseMigrator));

        return services;
    }

    public static IServiceCollection AddDapperContext(this IServiceCollection services, DatabaseType databaseType)
    {
        services.AddSingleton<IQueries>(new Queries(databaseType));
        services.AddScoped<IDapperContext, DapperContext>();
        SqlMapper.AddTypeHandler(new UserId.DapperTypeHandler());
        SqlMapper.AddTypeHandler(new AlertId.DapperTypeHandler());
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

    public static IServiceCollection AddSingletonSession(this IServiceCollection services)
    {
        var session = RuntimeHelpers.GetUninitializedObject(typeof(Session)) as ISession;
        services.AddSingleton(session!);
        return services;
    }

    public static IServiceCollection AddIdHasher(this IServiceCollection services)
    {
        services.AddSingleton<IIdHasher, IdHasher.IdHasher>();
        return services;
    }

    public static IServiceCollection SetupMassTransit(this IServiceCollection services, IConfiguration config,
        Action<IBusRegistrationConfigurator>? configureConsumers = null)
    {
        var rabbitMqSettings = config.Get<RabbitMqSettings>();

        services.AddMassTransit(opts =>
        {
            opts.AddEntityFrameworkOutbox<SiteWatcherContext>(o =>
            {
                o.UsePostgres();
                o.UseBusOutbox();
                o.QueryDelay = TimeSpan.FromSeconds(60);
                o.DuplicateDetectionWindow = TimeSpan.FromMinutes(5);
                o.QueryMessageLimit = 100;
            });

            // opts.AddDelayedMessageScheduler();

            configureConsumers?.Invoke(opts);
            opts.SetEndpointNameFormatter(new CustomEndpointNameFormatter());

            opts.UsingRabbitMq((b, cfg) =>
            {
                cfg.Host(rabbitMqSettings!.Host, rabbitMqSettings.Port, rabbitMqSettings.VirtualHost, o =>
                {
                    o.Password(rabbitMqSettings.Password);
                    o.Username(rabbitMqSettings.UserName);
                    o.PublisherConfirmation = true;
                });
                cfg.PrefetchCount = 10;
                cfg.ConcurrentMessageLimit = 3;

                // cfg.UseScheduledRedelivery(r =>
                //     r.Interval(3, TimeSpan.FromMinutes(5)));

                cfg.UseMessageRetry(r =>
                    r.Intervals(TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(15)));

                // cfg.UseDelayedMessageScheduler();
                cfg.ConfigureEndpoints(b);
            });
        });

        return services;
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

    public static IServiceCollection AddAuthService(this IServiceCollection services)
    {
        services.AddScoped<IAuthService,AuthService>();
        return services;
    }

    public static IServiceCollection SetupEmail(this IServiceCollection services, EmailSettings settings)
    {
        services
            .AddSingleton<EmailSettings>(settings)
            .AddScoped<IEmailService, EmailService>()
            .AddSingleton<EmailThrottler>(_ =>
            {
                var session = new Session(null!);
                return new EmailThrottler(settings, session);
            })
            .AddTransient<ISmtpClient, SmtpClient>();
        return services;
    }
}