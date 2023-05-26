using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Domain.Authentication;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.IdentityModel.Tokens;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Domain.Authentication;
using SiteWatcher.Domain.Authentication.Services;
using SiteWatcher.Domain.Common;
using SiteWatcher.Domain.Common.Constants;
using SiteWatcher.Domain.Common.Services;
using SiteWatcher.Domain.Common.ValueObjects;
using SiteWatcher.Domain.Users;
using SiteWatcher.Domain.Users.DTOs;
using SiteWatcher.Infra.Authorization.Constants;
using SiteWatcher.Infra.Authorization.Enums;

namespace SiteWatcher.Infra.Authorization;

public sealed class AuthService : IAuthService
{
    private readonly IAppSettings _appSettings;
    private readonly ICache _cache;
    private readonly ITimeLimitedDataProtector _protector;

    private const int RegisterTokenExpiration = 15 * 60;
    private const int LoginTokenExpiration = 8 * 60 * 60;
    private const int EmailConfirmationTokenExpiration = 24 * 60 * 60;
    private const int AccountReactivationTokenExpiration = 24 * 60 * 60;
    private const int LoginStateExpiration = 15 * 60 * 60;
    private const int AuthResExpiration = 20;

    public AuthService(IAppSettings appSettings, ICache cache, IDataProtectionProvider protector)
    {
        _appSettings = appSettings;
        _cache = cache;
        _protector = protector.CreateProtector(nameof(AuthService)).ToTimeLimitedDataProtector();
    }

    public string GenerateLoginToken(UserViewModel userVm)
    {
        var claims = new Claim[]
        {
            new(AuthenticationDefaults.ClaimTypes.Id, userVm.Id.ToString())
        };

        return GenerateToken(claims, TokenPurpose.Login, LoginTokenExpiration);
    }

    public string GenerateLoginToken(User user)
    {
        var claims = new Claim[]
        {
            new(AuthenticationDefaults.ClaimTypes.Id, user.Id.ToString()),
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

    public async Task InvalidateCurrenUser(ISession session)
    {
        var key = CacheKeys.InvalidUser(session.UserId!.Value);
        var whiteListedTokens = await _cache.GetAsync<List<string>>(key) ?? new List<string>();

        // Remove the current token from whitelist
        if (whiteListedTokens.Count > 0)
            whiteListedTokens.Remove(session.AuthTokenPayload);

        await _cache.SaveAsync(key, whiteListedTokens, TimeSpan.FromSeconds(LoginTokenExpiration));
    }

    public async Task<bool> UserCanLogin(UserId? userId, string authTokenPayload)
    {
        if (!userId.HasValue || UserId.Empty.Equals(userId))
            return false;

        var key = CacheKeys.InvalidUser(userId.Value);
        var whiteListedTokens = await _cache.GetAsync<List<string>>(key);

        // There is no whitelisted tokens, so the user was not invalidated
        if (whiteListedTokens is null)
            return true;

        // If there is a whitelisted tokens list, the user was invalidated by logging out of all devices,
        // or by being deleted, or by admin. Then we need to check if the current token payload is whitelisted.
        var currentTokenWhiteListed = whiteListedTokens.Contains(authTokenPayload);
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

    public async Task WhiteListTokenForCurrentUser(ISession session, string token) =>
        await WhiteListToken(session.UserId!.Value, token);

    public async Task InvalidateCurrentRegisterToken(ISession session)
    {
        await _cache.SaveBytesAsync(
            session.AuthTokenPayload,
            _appSettings.InvalidToken,
            TimeSpan.FromSeconds(RegisterTokenExpiration));
    }

    public async Task<bool> IsRegisterTokenValid(string authTokenPayload)
    {
        if (string.IsNullOrEmpty(authTokenPayload)) return false;
        var key = authTokenPayload;
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

    public async Task<AuthCodeResult> StoreAuthenticationResult(AuthenticationResult authRes, CancellationToken ct)
    {
        var codeKey = Utils.GenerateSafeRandomBase64String();
        var code = _protector.Protect(codeKey, TimeSpan.FromSeconds(AuthResExpiration));
        var authCodeRes = new AuthCodeResult(code);
        await _cache.SaveAsync(codeKey, authRes, TimeSpan.FromSeconds(AuthResExpiration));
        return authCodeRes;
    }

    public async Task<AuthenticationResult?> GetAuthenticationResult(string code, CancellationToken ct)
    {
        var codeKey = string.Empty;
        try
        {
            codeKey = _protector.Unprotect(code);
        }
        catch
        {
            // swallow unprotect exceptions
        }

        if (string.IsNullOrEmpty(codeKey)) return null;
        return await _cache.GetAndRemoveAsync<AuthenticationResult?>(codeKey, ct);
    }
}