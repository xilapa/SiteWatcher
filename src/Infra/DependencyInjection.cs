using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Domain.Models.Common;
using SiteWatcher.Infra.Authorization;
using SiteWatcher.Infra.Cache;
using SiteWatcher.Infra.DapperRepositories;
using SiteWatcher.Infra.Email;
using SiteWatcher.Infra.FireAndForget;
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
        configOptions.AbortOnConnectFail = false;
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
}