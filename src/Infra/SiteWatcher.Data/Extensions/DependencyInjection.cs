using SiteWatcher.Domain.Interfaces;
using SiteWatcher.Infra.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SiteWatcher.Data.DapperRepositories;
using StackExchange.Redis;
using SiteWatcher.Infra.Cache;
using Microsoft.EntityFrameworkCore.Migrations;
using SiteWatcher.Infra.Data;
using System.Text.RegularExpressions;

namespace SiteWatcher.Infra.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddDataContext<TContext>(this IServiceCollection services,  bool isDevelopment, string connectionString = null) where TContext : DbContext, IUnityOfWork
    {

        if(!isDevelopment)
        {
            var regexString = new Regex(@"\:\/\/(?<user>.*?)\:(?<password>.*?)\@(?<host>.*?)\:(?<port>.*?)\/(?<database>.*)");
            var matches = regexString.Match(connectionString).Groups;
            connectionString = $"Server={matches["host"].Value};" +
                                    $"Port={matches["port"].Value};" +
                                    $"Database={matches["database"].Value};" +
                                    $"User Id={matches["user"].Value};" +
                                    $"Password={matches["password"].Value}";
        }

        var optionsBuilder = new DbContextOptionsBuilder<TContext>();
        if(isDevelopment)
        {
            optionsBuilder.EnableSensitiveDataLogging();
            optionsBuilder.LogTo(Console.WriteLine);
        }

        Action<DbContextOptionsBuilder> optionsAction;
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

    public static IServiceCollection AddDapperRepositories(this IServiceCollection services, string connectionString)
    {
        services.AddScoped<IUserDapperRepository>(s => new UserDapperRepository(connectionString));
        return services;
    }

    public static IServiceCollection AddRedisCache(this IServiceCollection services, string connectionString)
    {
        var configOptions = ConfigurationOptions.Parse(connectionString);        
        configOptions.AbortOnConnectFail = false;
        configOptions.ConnectRetry = 3;
        configOptions.ConnectTimeout = 2_000;
        configOptions.ReconnectRetryPolicy = new ExponentialRetry(TimeSpan.FromSeconds(5).Milliseconds, TimeSpan.FromSeconds(20).Milliseconds);

        services.AddSingleton<IConnectionMultiplexer>(s => ConnectionMultiplexer.Connect(configOptions));
        services.AddSingleton<ICache, RedisCache>();
        return services;
    }
}