using AFA.Application.Interfaces;
using AFA.Application.Services;
using AFA.Domain.Interfaces;
using AFA.Domain.Services;
using Microsoft.Extensions.DependencyInjection;

namespace AFA.WebAPI.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IUserAppService, UserAppService>();
        return services;
    }

    public static IServiceCollection AddDomainServices(this IServiceCollection services)
    {
        services.AddScoped<IUserService, UserService>();
        return services;
    }
}