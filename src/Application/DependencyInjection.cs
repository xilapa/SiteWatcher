using System.Reflection;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using SiteWatcher.Application;
using SiteWatcher.Application.Users.Commands.RegisterUser;

namespace Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddAutoMapper(Assembly.GetAssembly(typeof(AutoMapperProfile)));
        services.AddMediatR(typeof(RegisterUserCommand));
        return services;
    }
}