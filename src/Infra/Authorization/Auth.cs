using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Domain.Common.Constants;
using SiteWatcher.Infra.Authorization.Constants;
using SiteWatcher.Infra.Authorization.GoogleAuth;
using SiteWatcher.Infra.Authorization.Handlers;
using SiteWatcher.Infra.Authorization.Middleware;

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

        // Bind the token issuer to the authentication scheme
        MultipleJwtsMiddleware.RegisterIssuer(AuthenticationDefaults.Issuers.Login,
            AuthenticationDefaults.Schemes.Login);
        MultipleJwtsMiddleware.RegisterIssuer(AuthenticationDefaults.Issuers.Register,
            AuthenticationDefaults.Schemes.Register);

        services.AddAuthorization(opts =>
        {
            // Do not set the AuthenticationSchemes on any policy, the MultipleJwtMiddleware will handle jwt
            // authentication checking the issuer, if the user was not authenticated by the default scheme.
            // If AuthenticationSchemes was set on a policy, the token will be validated twice
            // Also, doesn't need to require authenticated users, since the MultipleJwtMiddleware always requires
            // authentication for routes that doesn't allow anonymous users

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