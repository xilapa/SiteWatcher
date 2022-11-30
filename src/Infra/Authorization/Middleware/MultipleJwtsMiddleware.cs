using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace SiteWatcher.Infra.Authorization.Middleware;

public class MultipleJwtsMiddleware
{
    private readonly RequestDelegate _next;
    private static readonly object SchemesLock = new();
    private static readonly Dictionary<string, string> Schemes = new();

    public MultipleJwtsMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        var authenticated = await TryAuthenticateUser(context);
        if (authenticated)
        {
            await _next(context);
            return;
        }

        context.Response.StatusCode = 401;
    }

    /// <summary>
    /// Returns true if the user was authenticated or the route allow anonymous access
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    private static async Task<bool> TryAuthenticateUser(HttpContext context)
    {
        // If it is already authenticated with the default scheme, then just continue
        if (context.User.Identity!.IsAuthenticated)
            return true;

        // If the route allow anonymous users, then just continue
        if (context.GetEndpoint()?.Metadata.GetMetadata<IAllowAnonymous>() != null)
            return true;

        // If there is no auth token, then user is unauthorized
        var authHeader = context.Request.Headers.Authorization.ToString();
        if (string.IsNullOrEmpty(authHeader))
            return false;

        // If the auth header doesn't have the minimum lenght to split, then user is unauthorized
        if (authHeader.Length < 8)
            return false;

        var token = authHeader[7..];
        var handler = new JwtSecurityTokenHandler();

        // If it's not a jwt token, then user is unauthorized
        var isJwtToken = handler.CanReadToken(token);
        if (!isJwtToken)
            return false;

        var decodedToken = handler.ReadJwtToken(token);
        var issuer = decodedToken.Issuer;

        // If the token doesn't have an issuer, then user is unauthorized
        if (string.IsNullOrEmpty(issuer))
            return false;

        // If there is no registered scheme for this token, then user is unauthorized 
        var registeredScheme = Schemes.TryGetValue(issuer, out var scheme);
        if (!registeredScheme)
            return false;

        // if authentication was unsuccessful, then user is unauthorized 
        var authResult = await context.AuthenticateAsync(scheme);
        if (!authResult.Succeeded)
            return false;

        // Set the User on HttpContext
        context.User = authResult.Principal;
        return true;
    }

    public static void RegisterIssuer(string issuer, string scheme)
    {
        lock (SchemesLock)
        {
            Schemes.TryAdd(issuer, scheme);
        }
    }
}