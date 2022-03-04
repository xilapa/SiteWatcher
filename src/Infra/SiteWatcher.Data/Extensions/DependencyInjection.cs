using SiteWatcher.Domain.Interfaces;
using SiteWatcher.Infra.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace SiteWatcher.Infra.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddDataContext<TContext>(this IServiceCollection services,  bool isDevelopment, string connectionString = null) where TContext : DbContext, IUnityOfWork
    {
        var optionsBuilder = new DbContextOptionsBuilder<TContext>();
        if(isDevelopment)
        {
            optionsBuilder.EnableSensitiveDataLogging();
            optionsBuilder.LogTo(Console.WriteLine);
        }

        Action<DbContextOptionsBuilder> optionsAction;
        optionsAction = connectionString is null ? null : options => options.UseNpgsql(connectionString);

        // Explicitando que o contexto é o mesmo para todos os repositórios
        services.AddDbContext<TContext>(optionsAction, ServiceLifetime.Scoped); 
        services.AddScoped<IUnityOfWork>(f => f.GetRequiredService<TContext>());

        return services;
    }

    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IUserRepository, UserRepository>();
        return services;
    }
}