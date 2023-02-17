using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Domain.Common.Constants;
using SiteWatcher.Domain.Common.Services;
using SiteWatcher.Infra.Authorization.Constants;
using SiteWatcher.Infra.Authorization.GoogleAuth;
using SiteWatcher.Infra.Authorization.Handlers;

namespace SiteWatcher.Infra.Authorization;

public static class Auth
{
    public static IServiceCollection ConfigureAuth(this IServiceCollection services, IAppSettings appSettings, IConfiguration configuration)
    {
        var googleSettings = configuration.Get<GoogleSettings>();
        services.AddAuthentication(AuthenticationDefaults.Schemas.Cookie)
            .AddCookie(AuthenticationDefaults.Schemas.Cookie,
                opt =>
                {
                    opt.Cookie.Name = AuthenticationDefaults.CookieName;
                    opt.Cookie.HttpOnly = true;
                    opt.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                    opt.Cookie.IsEssential = true;
                    opt.Cookie.SameSite = SameSiteMode.None;
                    opt.Events.OnRedirectToLogin = (context) =>
                    {
                        context.Response.StatusCode = 401;
                        return Task.CompletedTask;
                    };
                })
            .AddGoogle(AuthenticationDefaults.Schemas.Google, opts =>
            {
                opts.SignInScheme = AuthenticationDefaults.Schemas.Cookie;
                opts.ClientId = googleSettings.ClientId;
                opts.ClientSecret = googleSettings.ClientSecret;
                opts.ClaimActions.MapJsonKey(AuthenticationDefaults.ClaimTypes.ProfilePicUrl, AuthenticationDefaults.Google.ProfilePicture);
                opts.ClaimActions.MapJsonKey(ClaimTypes.Locality, AuthenticationDefaults.Google.Locale);
            });
        // // Bind the token issuer to the authentication scheme
        // MultipleJwtsMiddleware.RegisterIssuer(AuthenticationDefaults.Issuers.Login,
        //     AuthenticationDefaults.Schemas.Login);
        // MultipleJwtsMiddleware.RegisterIssuer(AuthenticationDefaults.Issuers.Register,
        //     AuthenticationDefaults.Schemas.Register);

        services.AddAuthorization(opts =>
        {
            // Do not set the AuthenticationSchemes on any policy, the MultipleJwtMiddleware will handle jwt
            // authentication checking the issuer, if the user was not authenticated by the default scheme.
            // If AuthenticationSchemes was set on a policy, the token will be validated twice
            // Also, doesn't need to require authenticated users, since the MultipleJwtMiddleware always requires
            // authentication for routes that doesn't allow anonymous users

            // Only runs if no policy was specified on the authorize attribute
            // opts.DefaultPolicy = new AuthorizationPolicyBuilder()
            //     .AddRequirements(new ValidAuthData())
            //     .Build();

            // opts.AddPolicy(Policies.ValidRegisterData,
            //     policy => policy.AddRequirements(new ValidRegisterData()));
            opts.DefaultPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();
        });

        services.AddScoped<IAuthService, AuthService>();

        services.AddScoped<IAuthorizationHandler, ValidAuthDataHandler>();
        services.AddScoped<IAuthorizationHandler, ValidRegisterDataHandler>();

        return services;
    }
}