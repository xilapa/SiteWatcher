using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Common.Services;
using SiteWatcher.Domain.Common.Constants;
using SiteWatcher.Infra.Authorization.Constants;
using SiteWatcher.Infra.Authorization.GoogleAuth;
using SiteWatcher.Infra.Authorization.Handlers;

namespace SiteWatcher.Infra.Authorization;

public static class Auth
{
    public static IServiceCollection ConfigureAuth(this IServiceCollection services, IAppSettings appSettings)
    {
        services.AddAuthentication(AuthenticationDefaults.Schemes.Login)
            .AddJwtBearer(AuthenticationDefaults.Schemes.Login, opts =>
            {
                opts.MapInboundClaims = false;
                opts.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ClockSkew = TimeSpan.FromSeconds(0),
                    ValidateAudience = false,
                    ValidateIssuer = true,
                    ValidIssuers = new []{ AuthenticationDefaults.Issuers.Login },
                    RoleClaimType = AuthenticationDefaults.Roles,
                    IssuerSigningKey = new SymmetricSecurityKey(appSettings.AuthKey)
                };
            })
            .AddJwtBearer(AuthenticationDefaults.Schemes.Register, opts =>
            {
                opts.MapInboundClaims = false;
                opts.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ClockSkew = TimeSpan.FromSeconds(0),
                    ValidateAudience = false,
                    ValidateIssuer = true,
                    ValidIssuers = new []{ AuthenticationDefaults.Issuers.Register },
                    RoleClaimType = AuthenticationDefaults.Roles,
                    IssuerSigningKey = new SymmetricSecurityKey(appSettings.RegisterKey)
                };
            });

        services.AddAuthorization(opts =>
        {
            // Only runs if no policy was specified on the authorize attribute
            opts.DefaultPolicy = new AuthorizationPolicyBuilder()
                .AddRequirements(new ValidAuthData())
                .Build();

            opts.AddPolicy(Policies.ValidRegisterData,
                policy => policy.AddRequirements(new ValidRegisterData()));
        });

        services.AddScoped<IAuthService,AuthService>();
        services.AddScoped<IGoogleAuthService,GoogleAuthService>();

        services.AddScoped<IAuthorizationHandler, ValidAuthDataHandler>();
        services.AddScoped<IAuthorizationHandler, ValidRegisterDataHandler>();

        return services;
    }
}