using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Domain.Authentication;
using SiteWatcher.Domain.Common;
using SiteWatcher.Domain.Common.Constants;
using SiteWatcher.Domain.Common.Enums;
using SiteWatcher.Domain.Common.Extensions;
using SiteWatcher.Domain.Common.Services;
using SiteWatcher.Domain.Common.ValueObjects;
using SiteWatcher.Domain.Users.DTOs;
using SiteWatcher.Domain.Users.Enums;
using SiteWatcher.Infra.Authorization.Constants;

namespace SiteWatcher.Infra.Authorization;

public class AuthService : IAuthService
{
    private readonly IAppSettings _appSettings;
    private readonly ICache _cache;
    private readonly ISession _session;

    private const int AuthSessionExpiration = 30;
    private const int RegisterTokenExpiration = 15 * 60;
    private const int LoginTokenExpiration = 8 * 60 * 60;
    private const int EmailConfirmationTokenExpiration = 24 * 60 * 60;
    private const int AccountReactivationTokenExpiration = 24 * 60 * 60;
    private const int LoginStateExpiration = 15 * 60 * 60;

    public AuthService(IAppSettings appSettings, ICache cache, ISession session)
    {
        _appSettings = appSettings;
        _cache = cache;
        _session = session;
    }

    public string GenerateRegisterToken(string googleId, string name, string email, string locale)
    {
        var claims = new[]
        {
            new Claim(AuthenticationDefaults.ClaimTypes.Name, name),
            new Claim(AuthenticationDefaults.ClaimTypes.Email, email),
            new(AuthenticationDefaults.ClaimTypes.Language, ((int) locale!.GetEnumValue<Language>()).ToString()),
            new(AuthenticationDefaults.ClaimTypes.GoogleId, googleId)
        };

        return GenerateToken(claims, TokenPurpose.Register, RegisterTokenExpiration);
    }

    private string GenerateToken(IEnumerable<Claim> claims, TokenPurpose tokenPurpose, int expiration)
    {
        var key = tokenPurpose switch
        {
            TokenPurpose.Register => _appSettings.RegisterKey,
            TokenPurpose.Login => _appSettings.AuthKey,
            _ => throw new ArgumentException("Value out of range", nameof(tokenPurpose)),
        };

        var issuer = tokenPurpose switch
        {
            TokenPurpose.Register => AuthenticationDefaults.Issuers.Register,
            TokenPurpose.Login => AuthenticationDefaults.Issuers.Login,
            _ => throw new ArgumentException("Value out of range", nameof(tokenPurpose)),
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Issuer = issuer,
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddSeconds(expiration),
            SigningCredentials =
                new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(token);

        return tokenString;
    }

    public async Task InvalidateCurrenUser()
    {
        var key = CacheKeys.InvalidUser(_session.UserId);
        var whiteListedTokens = await _cache.GetAsync<List<string>>(key) ?? new List<string>();

        // Remove the current token from whitelist
        if (whiteListedTokens.Count > 0)
            whiteListedTokens.Remove("a");

        await _cache.SaveAsync(key, whiteListedTokens, TimeSpan.FromSeconds(LoginTokenExpiration));
    }

    public async Task<bool> UserCanLogin()
    {
        var key = CacheKeys.InvalidUser(_session.UserId);
        var whiteListedTokens = await _cache.GetAsync<List<string>>(key);

        // There is no whitelisted tokens, so the user was not invalidated
        if (whiteListedTokens is null)
            return true;

        // If there is a whitelisted tokens list, the user was invalidated by logging out of all devices,
        // or by being deleted, or by admin. Then we need to check if the current token payload is whitelisted.
        var currentTokenWhiteListed = whiteListedTokens.Contains("a");
        return currentTokenWhiteListed;
    }

    public Task WhiteListToken(UserId userId, string token)
    {
        // TODO: re-implement this
        // var key = CacheKeys.InvalidUser(userId);
        // var whiteListedTokens = await _cache.GetAsync<List<string>>(key);

        // // There is no whitelisted tokens, so the user was not invalidated
        // if (whiteListedTokens is null)
        //     return;


        // whiteListedTokens.Add(tokenPayload);
        // await _cache.SaveAsync(key, whiteListedTokens, TimeSpan.FromSeconds(LoginTokenExpiration));
        return Task.CompletedTask;
    }

    public async Task WhiteListTokenForCurrentUser(string token) =>
        await WhiteListToken(_session.UserId, token);

    public async Task InvalidateCurrentRegisterToken()
    {
        await _cache.SaveBytesAsync(
            "aaa",
            _appSettings.InvalidToken,
            TimeSpan.FromSeconds(RegisterTokenExpiration));
    }

    public async Task<bool> IsRegisterTokenValid()
    {
        var key = "_session.AuthTokenPayload;";
        var value = await _cache.GetBytesAsync(key);
        return !_appSettings.InvalidToken.SequenceEqual(value ?? Array.Empty<byte>());
    }

    public async Task<string> SetEmailConfirmationTokenExpiration(string token, UserId userId)
    {
        await _cache.SaveStringAsync(token, userId.Value.ToString(),
            TimeSpan.FromSeconds(EmailConfirmationTokenExpiration));
        return token;
    }

    public async Task<UserId?> GetUserIdFromConfirmationToken(string token)
    {
        var userIdString = await _cache.GetAndRemoveStringAsync(token);
        if (string.IsNullOrEmpty(userIdString))
            return null;
        var userId = new UserId(new Guid(userIdString));
        return userId;
    }

    public async Task<string> SetAccountActivationTokenExpiration(string token, UserId userId)
    {
        await _cache.SaveStringAsync(token, userId.Value.ToString(),
            TimeSpan.FromSeconds(AccountReactivationTokenExpiration));
        return token;
    }

    public async Task<string> CreateLoginAuthSession(UserViewModel user, string? profilePictureUrl = null)
    {
        var session = new AuthSession(AuthTask.Login, user, profilePictureUrl);
        var sessionKey = Utils.GenerateSafeRandomBase64String();
        await _cache.SaveAsync(sessionKey, session, TimeSpan.FromSeconds(AuthSessionExpiration));
        return sessionKey;
    }

    public async Task<string> CreateRegisterAuthSession(string googleId, string name, string email, string locale, string? profilePictureUrl = null)
    {
        var language = Utils.GetLanguageFromLocale(locale);
        var session = new AuthSession(AuthTask.Register, googleId, name, email, language, profilePictureUrl);
        var sessionKey = Utils.GenerateSafeRandomBase64String();
        await _cache.SaveAsync(sessionKey, session, TimeSpan.FromSeconds(AuthSessionExpiration));
        return sessionKey;
    }

    public async Task<string> CreateActivateAuthSession(UserViewModel user)
    {
        var session = new AuthSession(AuthTask.Activate, user);
        var sessionKey = Utils.GenerateSafeRandomBase64String();
        await _cache.SaveAsync(sessionKey, session, TimeSpan.FromSeconds(AuthSessionExpiration));
        return sessionKey;
    }

    public async Task<SessionView> GenerateSession(string token)
    {
        // check if user has an authentication session
        // TODO: implement a get and remove method on cache
        var authSession = await _cache.GetAsync<AuthSession>(token);

        if (authSession is null) return InvalidSession;

        // TODO: protect sessionId on sessionView

        if (authSession.Task == AuthTask.Register)
        {
            var key = Utils.GenerateSafeRandomBase64String();
            authSession.RegisterToken = key;
            await _cache.SaveAsync(key, authSession, TimeSpan.FromSeconds(RegisterTokenExpiration));
            return new SessionView(authSession, key);
        }

        // renew activate auth session, front end will call send activation email that will check if this session exists
        if (authSession.Task == AuthTask.Activate)
        {
            var key = Utils.GenerateSafeRandomBase64String();
            authSession.RegisterToken = key;
            await _cache.SaveAsync(key, authSession, TimeSpan.FromSeconds(AuthSessionExpiration));
            return new SessionView(authSession, key);
        }

        // check if user already has a session
        var sessionKey = CacheKeys.UserSession(authSession.UserId!.Value);
        var session = await _cache.GetAsync<Session>(sessionKey);

        var sessionId = Utils.GenerateSafeRandomBase64String();
        session ??= Session.Create(authSession);

        session.AddSessionId(sessionId);

        await _cache.SaveAsync(sessionKey, session, TimeSpan.FromSeconds(LoginTokenExpiration));
        return new SessionView(session, sessionId, authSession.Theme!.Value, authSession.ProfilePicUrl);
    }

    private static readonly SessionView InvalidSession = new SessionView(AuthTask.Error);
}