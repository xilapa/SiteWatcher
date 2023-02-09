using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Common.Services;
using SiteWatcher.Domain.Common;
using SiteWatcher.Domain.Common.Constants;
using SiteWatcher.Domain.Common.Enums;
using SiteWatcher.Domain.Common.Extensions;
using SiteWatcher.Domain.Common.Services;
using SiteWatcher.Domain.Common.ValueObjects;
using SiteWatcher.Domain.Users;
using SiteWatcher.Domain.Users.DTOs;
using SiteWatcher.Domain.Users.Enums;
using SiteWatcher.Infra.Authorization.Constants;

namespace SiteWatcher.Infra.Authorization;

public class AuthService : IAuthService
{
    private readonly IAppSettings _appSettings;
    private readonly ICache _cache;
    private readonly ISession _session;

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

    public string GenerateLoginToken(UserViewModel userVm)
    {
        var claims = new Claim[]
        {
            new(AuthenticationDefaults.ClaimTypes.Id, userVm.Id.ToString()),
            new(AuthenticationDefaults.ClaimTypes.Name, userVm.Name),
            new(AuthenticationDefaults.ClaimTypes.Email, userVm.Email),
            new(AuthenticationDefaults.ClaimTypes.EmailConfirmed, userVm.EmailConfirmed.ToString().ToLower()),
            new(AuthenticationDefaults.ClaimTypes.Language, ((int) userVm.Language).ToString()),
            new(AuthenticationDefaults.ClaimTypes.Theme, ((int) userVm.Theme).ToString())
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
            new(AuthenticationDefaults.ClaimTypes.Language, ((int) user.Language).ToString()),
            new(AuthenticationDefaults.ClaimTypes.Theme, ((int) user.Theme).ToString())
        };

        return GenerateToken(claims, TokenPurpose.Login, LoginTokenExpiration);
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
        if(string.IsNullOrEmpty(userIdString))
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
}