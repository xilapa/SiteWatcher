using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AFA.Data.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddDataContext<TContext>(this IServiceCollection services) where TContext : DbContext
    {
        services.AddDbContext<TContext>();
        return services;
    }

    // TODO: criar sobrecarga que receba connection string
}