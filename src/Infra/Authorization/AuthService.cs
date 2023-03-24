using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Domain.Authentication;
using Domain.Common.Services;
using Microsoft.IdentityModel.Tokens;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Domain.Authentication;
using SiteWatcher.Domain.Authentication.Services;
using SiteWatcher.Domain.Common;
using SiteWatcher.Domain.Common.Constants;
using SiteWatcher.Domain.Common.Enums;
using SiteWatcher.Domain.Common.ValueObjects;
using SiteWatcher.Domain.Users;
using SiteWatcher.Domain.Users.DTOs;
using SiteWatcher.Infra.Authorization.Constants;

namespace SiteWatcher.Infra.Authorization;

public sealed class AuthService : IAuthService
{
    private readonly IAppSettings _appSettings;
    private readonly ICache _cache;
    private readonly ISession _session;
    private readonly IDataProtectorService _protector;

    private const int RegisterTokenExpiration = 15 * 60;
    private const int LoginTokenExpiration = 8 * 60 * 60;
    private const int EmailConfirmationTokenExpiration = 24 * 60 * 60;
    private const int AccountReactivationTokenExpiration = 24 * 60 * 60;
    private const int LoginStateExpiration = 15 * 60 * 60;
    private const int AuthResExpiration = 20;

    public AuthService(IAppSettings appSettings, ICache cache, ISession session,
        IDataProtectorService protector)
    {
        _appSettings = appSettings;
        _cache = cache;
        _session = session;
        _protector = protector;
    }

    public string GenerateLoginToken(UserViewModel userVm)
    {
        var claims = new Claim[]
        {
            new(AuthenticationDefaults.ClaimTypes.Id, userVm.Id.ToString()),
            new(AuthenticationDefaults.ClaimTypes.Name, userVm.Name),
            new(AuthenticationDefaults.ClaimTypes.Email, userVm.Email),
            new(AuthenticationDefaults.ClaimTypes.EmailConfirmed, userVm.EmailConfirmed.ToString().ToLower()),
            new(AuthenticationDefaults.ClaimTypes.Language, ((int)userVm.Language).ToString()),
            new(AuthenticationDefaults.ClaimTypes.Theme, ((int)userVm.Theme).ToString())
        };

        return GenerateToken(claims, TokenPurpose.Login, LoginTokenExpiration);
    }

    public string GenerateLoginToken(User user)
    {
        var claims = new Claim[]
        {
            new(AuthenticationDefaults.ClaimTypes.Id, user.Id.ToString()),
            new(AuthenticationDefaults.ClaimTypes.Name, user.Name),
            new(AuthenticationDefaults.ClaimTypes.Email, user.Email),
            new(AuthenticationDefaults.ClaimTypes.EmailConfirmed, user.EmailConfirmed.ToString().ToLower()),
            new(AuthenticationDefaults.ClaimTypes.Language, ((int)user.Language).ToString()),
            new(AuthenticationDefaults.ClaimTypes.Theme, ((int)user.Theme).ToString())
        };

        return GenerateToken(claims, TokenPurpose.Login, LoginTokenExpiration);
    }

    public string GenerateRegisterToken(UserRegisterData user)
    {
        var claims = new Claim[]
        {
            new(AuthenticationDefaults.ClaimTypes.Name, user.Name ?? string.Empty),
            new(AuthenticationDefaults.ClaimTypes.Email, user.Email),
            new(AuthenticationDefaults.ClaimTypes.Language, ((int)user.Language()).ToString()),
            new(AuthenticationDefaults.ClaimTypes.GoogleId, user.GoogleId)
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
        var key = CacheKeys.InvalidUser(_session.UserId!.Value);
        var whiteListedTokens = await _cache.GetAsync<List<string>>(key) ?? new List<string>();

        // Remove the current token from whitelist
        if (whiteListedTokens.Count > 0)
            whiteListedTokens.Remove(_session.AuthTokenPayload);

        await _cache.SaveAsync(key, whiteListedTokens, TimeSpan.FromSeconds(LoginTokenExpiration));
    }

    public async Task<bool> UserCanLogin()
    {
        var key = CacheKeys.InvalidUser(_session.UserId!.Value);
        var whiteListedTokens = await _cache.GetAsync<List<string>>(key);

        // There is no whitelisted tokens, so the user was not invalidated
        if (whiteListedTokens is null)
            return true;

        // If there is a whitelisted tokens list, the user was invalidated by logging out of all devices,
        // or by being deleted, or by admin. Then we need to check if the current token payload is whitelisted.
        var currentTokenWhiteListed = whiteListedTokens.Contains(_session.AuthTokenPayload);
        return currentTokenWhiteListed;
    }

    public async Task WhiteListToken(UserId userId, string token)
    {
        var key = CacheKeys.InvalidUser(userId);
        var whiteListedTokens = await _cache.GetAsync<List<string>>(key);

        // There is no whitelisted tokens, so the user was not invalidated
        if (whiteListedTokens is null)
            return;

        var tokenPayload = Utils.GetTokenPayload(token);
        whiteListedTokens.Add(tokenPayload);
        await _cache.SaveAsync(key, whiteListedTokens, TimeSpan.FromSeconds(LoginTokenExpiration));
    }

    public async Task WhiteListTokenForCurrentUser(string token) =>
        await WhiteListToken(_session.UserId!.Value, token);

    public async Task InvalidateCurrentRegisterToken()
    {
        await _cache.SaveBytesAsync(
            _session.AuthTokenPayload,
            _appSettings.InvalidToken,
            TimeSpan.FromSeconds(RegisterTokenExpiration));
    }

    public async Task<bool> IsRegisterTokenValid()
    {
        var key = _session.AuthTokenPayload;
        var value = await _cache.GetBytesAsync(key);
        return !_appSettings.InvalidToken.SequenceEqual(value ?? Array.Empty<byte>());
    }

    public async Task<string> GenerateLoginState(byte[] stateBytes)
    {
        var state = Utils.GenerateSafeRandomBase64String();
        await _cache.SaveBytesAsync(state, stateBytes, TimeSpan.FromSeconds(LoginStateExpiration));
        return state;
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

    public async Task<AuthKeys> StoreAuthenticationResult(AuthenticationResult authRes, CancellationToken ct)
    {
        var key = Utils.GenerateSafeRandomBase64String();
        var securityToken = _protector.Protect(key, TimeSpan.FromSeconds(AuthResExpiration));
        var authkeys = new AuthKeys(key, securityToken);
        await _cache.SaveAsync(key, authRes, TimeSpan.FromSeconds(AuthResExpiration));
        return authkeys;
    }

    public async Task<AuthenticationResult?> GetAuthenticationResult(string key, string token, CancellationToken ct)
    {
        var decodeToken = _protector.Unprotect(token);
        if (!key.Equals(decodeToken)) return null;
        return await _cache.GetAndRemoveAsync<AuthenticationResult?>(key, ct);
    }
}