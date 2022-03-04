using SiteWatcher.Domain.Interfaces;
using SiteWatcher.Domain.Services;
using SiteWatcher.Application.Validators;
using SiteWatcher.WebAPI.Settings;
using SiteWatcher.WebAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using SiteWatcher.WebAPI.Constants;
using System.Reflection;
using SiteWatcher.Application;
using MediatR;
using SiteWatcher.Application.Commands;

namespace SiteWatcher.WebAPI.Extensions;

public static class DependencyInjection
{
    // TODO: renomear este método
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddAutoMapper(Assembly.GetAssembly(typeof(AutoMapperProfile)));
        services.AddMediatR(typeof(RegisterUserCommand));
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

    public static IServiceCollection AddSettings(this IServiceCollection services, IConfiguration configuration)
    {
        // TODO: Mudar lifetime para transient e transformar chaves em byte[],
        // limpando a chave depois de utilizada. 
        // Com isso configuration.Get vai pra dentro das classes de settings.
        var googleSettings = configuration.Get<GoogleSettings>();
        var appSettings = configuration.Get<AppSettings>();
        services.AddSingleton(googleSettings);
        services.AddSingleton(appSettings);
        return services;
    }

    public static IServiceCollection ConfigureAuth(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<ITokenService,TokenService>();
        var appSettings = configuration.Get<AppSettings>();

        services.AddAuthentication(opts => {
            opts.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            opts.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, opts => {
            opts.MapInboundClaims = false;
            opts.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(appSettings.AuthKey)),
                ClockSkew = TimeSpan.FromSeconds(0),
                ValidateAudience = false,
                ValidateIssuer = false,
                RoleClaimType = AuthenticationDefaults.Roles
            };
        })
        .AddJwtBearer(AuthenticationDefaults.RegisterScheme, opts => {
            opts.MapInboundClaims = false;
            opts.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(appSettings.RegisterKey)),
                ClockSkew = TimeSpan.FromSeconds(0),
                ValidateAudience = false,
                ValidateIssuer = false,
                RoleClaimType = AuthenticationDefaults.Roles
            };
        });

        return services;
    }
}