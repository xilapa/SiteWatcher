using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Domain.Authentication.Services;
using SiteWatcher.Domain.Common.Constants;
using SiteWatcher.Infra.Authorization.Constants;
using SiteWatcher.Infra.Authorization.Handlers;

namespace SiteWatcher.Infra.Authorization;

public static class Auth
{
    public static IServiceCollection ConfigureAuth(this IServiceCollection services, IAppSettings appSettings, IGoogleSettings googleSettings)
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
                    ValidIssuers = new[] { AuthenticationDefaults.Issuers.Login },
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
                    ValidIssuers = new[] { AuthenticationDefaults.Issuers.Register },
                    RoleClaimType = AuthenticationDefaults.Roles,
                    IssuerSigningKey = new SymmetricSecurityKey(appSettings.RegisterKey)
                };
            })
            .AddCookie(AuthenticationDefaults.Schemes.Cookie, opt =>
            {
                opt.Cookie.Name = AuthenticationDefaults.Schemes.Cookie;
                opt.Cookie.HttpOnly = true;
                opt.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                opt.Cookie.IsEssential = true;
                opt.Cookie.SameSite = SameSiteMode.None;
                opt.Events.OnRedirectToLogin = ctx =>
                {
                    ctx.Response.StatusCode = 401;
                    return Task.CompletedTask;
                };
            })
            .AddGoogle(AuthenticationDefaults.Schemes.Google, opts =>
            {
                opts.SignInScheme = AuthenticationDefaults.Schemes.Cookie;
                opts.ClientId = googleSettings.ClientId;
                opts.ClientSecret  = googleSettings.ClientSecret;
                opts.ClaimActions.MapJsonKey(
                    AuthenticationDefaults.ClaimTypes.ProfilePicUrl, AuthenticationDefaults.Google.Picture
                    );
                opts.ClaimActions.MapJsonKey(AuthenticationDefaults.ClaimTypes.Locale, AuthenticationDefaults.Google.Locale);
            });

        services.AddAuthorization(opts =>
        {
            // Only runs if no policy was specified on the authorize attribute
            opts.DefaultPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .AddRequirements(new ValidAuthData())
                .AddAuthenticationSchemes(AuthenticationDefaults.Schemes.Login)
                .Build();

            opts.AddPolicy(Policies.ValidRegisterData,
                policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.AddRequirements(new ValidRegisterData());
                    policy.AddAuthenticationSchemes(AuthenticationDefaults.Schemes.Register);
                });

            opts.AddPolicy(Policies.AuthCookie, policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.AddAuthenticationSchemes(AuthenticationDefaults.Schemes.Cookie);
            });
        });

        services.AddScoped<IAuthService,AuthService>();

        services.AddScoped<IAuthorizationHandler, ValidAuthDataHandler>();
        services.AddScoped<IAuthorizationHandler, ValidRegisterDataHandler>();

        return services;
    }
}