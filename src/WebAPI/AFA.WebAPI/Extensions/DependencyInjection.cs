using Microsoft.Extensions.DependencyInjection;
using AFA.Application.Interfaces;
using AFA.Application.Services;
using AFA.Domain.Interfaces;
using AFA.Domain.Services;
using FluentValidation.AspNetCore;
using AFA.Application.Validators;

namespace AFA.WebAPI.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IUserAppService, UserAppService>();
        return services;
    }

    public static IMvcBuilder AddFluentValidations(this IMvcBuilder mvcServcies)
    {
        mvcServcies
            .AddFluentValidation(opt =>
            {
                opt.RegisterValidatorsFromAssemblyContaining<ApplicationValidators>();
                opt.DisableDataAnnotationsValidation = true;
            });

        return mvcServcies;
    }

    public static IServiceCollection AddDomainServices(this IServiceCollection services)
    {
        services.AddScoped<IUserService, UserService>();
        return services;
    }
}