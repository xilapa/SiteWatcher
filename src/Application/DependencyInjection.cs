using System.Reflection;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using SiteWatcher.Application.Common.Mappings;
using SiteWatcher.Application.Users.Commands.RegisterUser;

namespace SiteWatcher.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddAutoMapper(Assembly.GetAssembly(typeof(AutoMapperProfile)));
        services.AddMediatR(typeof(RegisterUserCommand));
        return services;
    }
}