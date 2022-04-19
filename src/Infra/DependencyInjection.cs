using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Data.Cache;
using SiteWatcher.Infra.DapperRepositories;
using SiteWatcher.Infra.Repositories;
using StackExchange.Redis;

namespace SiteWatcher.Infra;

public static class DependencyInjection
{
    // TODO: Connection String null não faz tanto sentido mais
    public static IServiceCollection AddDataContext<TContext>(this IServiceCollection services,  bool isDevelopment, string? connectionString = null) where TContext : DbContext, IUnityOfWork
    {
        if(!isDevelopment && !string.IsNullOrEmpty(connectionString))
            connectionString = ParseHerokuConnectionString(connectionString);

        var optionsBuilder = new DbContextOptionsBuilder<TContext>();
        if(isDevelopment)
        {
            optionsBuilder.EnableSensitiveDataLogging();
            optionsBuilder.LogTo(Console.WriteLine);
        }

        Action<DbContextOptionsBuilder>? optionsAction;
        optionsAction = connectionString is null ? null :
                options => options.UseNpgsql(connectionString, x => x.MigrationsHistoryTable(HistoryRepository.DefaultTableName, SiteWatcherContext.Schema));

        // Making explicit that the context is the same for all repositories
        services.AddDbContext<TContext>(optionsAction, ServiceLifetime.Scoped); 
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

    public static IServiceCollection AddDapperRepositories(this IServiceCollection services, bool isDevelopment, string connectionString)
    {
        if(!isDevelopment)
            connectionString = ParseHerokuConnectionString(connectionString);

        services.AddScoped<IUserDapperRepository>(_ => new UserDapperRepository(connectionString));
        return services;
    }

    public static IServiceCollection AddRedisCache(this IServiceCollection services, string connectionString)
    {
        var configOptions = ConfigurationOptions.Parse(connectionString);
        configOptions.AbortOnConnectFail = false;
        configOptions.ConnectRetry = 3;
        configOptions.ConnectTimeout = 2_000;
        configOptions.ReconnectRetryPolicy = new ExponentialRetry(TimeSpan.FromSeconds(5).Milliseconds, TimeSpan.FromSeconds(20).Milliseconds);

        services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(configOptions));
        services.AddSingleton<ICache, RedisCache>();
        return services;
    }

    private static string ParseHerokuConnectionString(string connectionString)
    {
            var regexString = new Regex(@"\:\/\/(?<user>.*?)\:(?<password>.*?)\@(?<host>.*?)\:(?<port>.*?)\/(?<database>.*)");
            var matches = regexString.Match(connectionString).Groups;
            return  $"Server={matches["host"].Value};" +
                    $"Port={matches["port"].Value};" +
                    $"Database={matches["database"].Value};" +
                    $"User Id={matches["user"].Value};" +
                    $"Password={matches["password"].Value}";
    }
}