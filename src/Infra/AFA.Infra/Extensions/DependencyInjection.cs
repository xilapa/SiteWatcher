using System;
using AFA.Domain.Interfaces;
using AFA.Infra.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AFA.Infra.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddDataContext<TContext>(this IServiceCollection services) where TContext : DbContext
    {
        // Explicitando que o contexto é o mesmo para todos os repositórios
        services.AddDbContext<TContext>(ServiceLifetime.Scoped); 

        return services;
    }

    // TODO: criar sobrecarga que receba connection string

    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IUserRepository, UserRepository>();
        return services;
    }
}