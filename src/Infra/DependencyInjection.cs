using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Data.Cache;
using SiteWatcher.Infra.Authorization;
using SiteWatcher.Infra.DapperRepositories;
using SiteWatcher.Infra.Repositories;
using StackExchange.Redis;

namespace SiteWatcher.Infra;

public static class DependencyInjection
{
    public static IServiceCollection AddDataContext<TContext>(this IServiceCollection services) where TContext : DbContext, IUnityOfWork
    {
        // Making explicit that the context is the same for all repositories
        services.AddDbContext<TContext>(ServiceLifetime.Scoped);
        services.AddScoped<IUnityOfWork>(s => s.GetRequiredService<TContext>());

        // Add migrator
        services.AddScoped(typeof(DatabaseMigrator));

        return services;
    }

    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IUserRepository, UserRepository>();
        return services;
    }

    public static IServiceCollection AddDapperRepositories(this IServiceCollection services)
    {
        services.AddScoped<IUserDapperRepository, UserDapperRepository>();
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

    public static IServiceCollection AddSessao(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ISessao, Sessao>();
        return services;
    }
}