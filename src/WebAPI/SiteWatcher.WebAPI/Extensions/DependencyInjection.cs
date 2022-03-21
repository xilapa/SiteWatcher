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
using System.IdentityModel.Tokens.Jwt;

namespace SiteWatcher.WebAPI.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddAutoMapper(Assembly.GetAssembly(typeof(AutoMapperProfile)));
        services.AddMediatR(typeof(RegisterUserCommand));
        return services;
    }

    public static IServiceCollection AddDomainServices(this IServiceCollection services)
    {
        services.AddScoped<IUserService, UserService>();
        return services;
    }

    public static IServiceCollection AddSettings(this IServiceCollection services)
    {
        services.AddSingleton(f => f.GetRequiredService<IConfiguration>().Get<GoogleSettings>());
        services.AddSingleton(f => f.GetRequiredService<IConfiguration>().Get<AppSettings>());
        return services;
    }

    public static IServiceCollection ConfigureAuth(this IServiceCollection services, AppSettings appSettings)
    {
        services.AddScoped<ITokenService,TokenService>();
        var servicesProvider = services.BuildServiceProvider();
        var tokenService = servicesProvider.GetRequiredService<ITokenService>();

        var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ClockSkew = TimeSpan.FromSeconds(0),
                ValidateAudience = false,
                ValidateIssuer = false,
                RoleClaimType = AuthenticationDefaults.Roles
            };

        services.AddAuthentication(opts => {
            opts.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            opts.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, opts => {
            opts.MapInboundClaims = false;
            tokenValidationParameters.IssuerSigningKey = new SymmetricSecurityKey(appSettings.AuthKey);
            opts.TokenValidationParameters = tokenValidationParameters;
        })
        .AddJwtBearer(AuthenticationDefaults.RegisterScheme, opts => {
            opts.MapInboundClaims = false;
            tokenValidationParameters.IssuerSigningKey = new SymmetricSecurityKey(appSettings.RegisterKey);
            opts.TokenValidationParameters = tokenValidationParameters;
        });

        return services;
    }
}