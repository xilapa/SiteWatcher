using Microsoft.Extensions.DependencyInjection;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Application.Services;
using SiteWatcher.Domain.Interfaces;
using SiteWatcher.Domain.Services;
using SiteWatcher.Application.Validators;

namespace SiteWatcher.WebAPI.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IUserAppService, UserAppService>();
        return services;
    }

    public static IServiceCollection AddApplicationFluentValidations(this IServiceCollection services)
    {
        Validator.LoadFluentValidators();
        return services;
    }

    public static IServiceCollection AddDomainServices(this IServiceCollection services)
    {
        services.AddScoped<IUserService, UserService>();
        return services;
    }
}