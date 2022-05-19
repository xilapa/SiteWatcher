using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Infra.Authorization.Constants;

namespace SiteWatcher.Infra.Authorization;

public static class Auth
{
    public static IServiceCollection ConfigureAuth(this IServiceCollection services, IAppSettings appSettings)
    {
        services.AddScoped<ITokenService,TokenService>();

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

        services.AddAuthorization();

        return services;
    }
}