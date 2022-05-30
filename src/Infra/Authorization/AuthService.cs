using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Domain.Enums;
using SiteWatcher.Domain.Extensions;
using SiteWatcher.Domain.Models;
using SiteWatcher.Domain.Models.Common;
using SiteWatcher.Domain.Utils;
using SiteWatcher.Domain.ViewModels;
using SiteWatcher.Infra.Authorization.Constants;

namespace SiteWatcher.Infra.Authorization;

public class AuthService : IAuthService
{
    private readonly IAppSettings _appSettings;
    private readonly ICache _cache;
    private readonly ISessao _sessao;

    private const int RegisterTokenExpiration = 15 * 60;
    private const int LoginTokenExpiration = 8 * 60 * 60;

    public AuthService(IAppSettings appSettings, ICache cache, ISessao sessao)
    {
        _appSettings = appSettings;
        _cache = cache;
        _sessao = sessao;
    }

    public string GenerateLoginToken(UserViewModel userVm)
    {
        var claims = new Claim[]
        {
            new (AuthenticationDefaults.ClaimTypes.Id, userVm.UserId.ToString()),
            new (AuthenticationDefaults.ClaimTypes.Name, userVm.Name),
            new (AuthenticationDefaults.ClaimTypes.Email, userVm.Email),
            new (AuthenticationDefaults.ClaimTypes.EmailConfirmed, userVm.EmailConfirmed.ToString().ToLower()),
            new (AuthenticationDefaults.ClaimTypes.Language, ((int)userVm.Language).ToString())
        };

        return GenerateToken(claims, ETokenPurpose.Login, LoginTokenExpiration);
    }

    public string GenerateLoginToken(User user)
    {
        var claims = new Claim[]
        {
            new (AuthenticationDefaults.ClaimTypes.Id, user.Id.ToString()),
            new (AuthenticationDefaults.ClaimTypes.Name, user.Name),
            new (AuthenticationDefaults.ClaimTypes.Email, user.Email),
            new (AuthenticationDefaults.ClaimTypes.EmailConfirmed, user.EmailConfirmed.ToString().ToLower()),
            new (AuthenticationDefaults.ClaimTypes.Language, ((int)user.Language).ToString())
        };

        return GenerateToken(claims, ETokenPurpose.Login, LoginTokenExpiration);
    }

    public string GenerateRegisterToken(IEnumerable<Claim> tokenClaims, string googleId)
    {
        var tokenClaimsEnumerated = tokenClaims as Claim[] ?? tokenClaims.ToArray();
        var locale = tokenClaimsEnumerated
                                    .DefaultIfEmpty(new Claim(AuthenticationDefaults.ClaimTypes.Locale, string.Empty))
                                    .FirstOrDefault(c => c.Type == AuthenticationDefaults.ClaimTypes.Locale)!.Value
                                    .Split("-").First();

        var claims = new []
        {
            tokenClaimsEnumerated.GetClaimValue(AuthenticationDefaults.ClaimTypes.Name),
            tokenClaimsEnumerated.GetClaimValue(AuthenticationDefaults.ClaimTypes.Email),
            new (AuthenticationDefaults.ClaimTypes.Language, ((int)locale.GetEnumValue<ELanguage>()).ToString()),
            new (AuthenticationDefaults.ClaimTypes.GoogleId, googleId)
        };

        return GenerateToken(claims, ETokenPurpose.Register, RegisterTokenExpiration);
    }

    private string GenerateToken(IEnumerable<Claim> claims, ETokenPurpose tokenPurpose, int expiration)
    {
        var key = tokenPurpose switch
        {
            ETokenPurpose.Register => _appSettings.RegisterKey,
            ETokenPurpose.Login => _appSettings.AuthKey,
            _ => throw new ArgumentException("Value out of range", nameof(tokenPurpose)),
        };

        var issuer = tokenPurpose switch
        {
            ETokenPurpose.Register => AuthenticationDefaults.Issuers.Register,
            ETokenPurpose.Login => AuthenticationDefaults.Issuers.Login,
            _ => throw new ArgumentException("Value out of range", nameof(tokenPurpose)),
        };

        var tokenDescriptor = new SecurityTokenDescriptor {
            Issuer = issuer,
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddSeconds(expiration),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(token);

        return tokenString;
    }

    public async Task InvalidateCurrenUser()
    {
        var key = CacheKeys.InvalidUser(_sessao.UserId!.Value);
        var whiteListedTokens = await _cache.GetAsync<List<string>>(key) ?? new List<string>();

        // Remove the current token from whitelist
        if(whiteListedTokens.Count > 0)
            whiteListedTokens.Remove(_sessao.AuthTokenPayload);

        await _cache.SaveAsync(key, whiteListedTokens, TimeSpan.FromSeconds(LoginTokenExpiration));
    }

    public async Task<bool> UserCanLogin()
    {
        var key = CacheKeys.InvalidUser(_sessao.UserId!.Value);
        var whiteListedTokens = await _cache.GetAsync<List<string>>(key);

        // There is no whitelisted tokens, so the user was not invalidated
        if (whiteListedTokens is null)
            return true;

        // If there is a whitelisted tokens list, the user was invalidated by logging out of all devices,
        // or by being deleted, or by admin. Then we need to check if the current token payload is whitelisted.
        var currentTokenWhiteListed = whiteListedTokens.Contains(_sessao.AuthTokenPayload);
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

    public async Task InvalidateCurrentRegisterToken()
    {
        await _cache.SaveBytesAsync(
            _sessao.AuthTokenPayload,
            _appSettings.InvalidToken,
            TimeSpan.FromSeconds(RegisterTokenExpiration));
    }

    public async Task<bool> IsRegisterTokenValid()
    {
        var key = _sessao.AuthTokenPayload;
        var value = await _cache.GetBytesAsync(key);
        return !_appSettings.InvalidToken.SequenceEqual(value ?? Array.Empty<byte>());
    }

    public async Task<string> GenerateLoginState(byte[] stateBytes)
    {
        var state = Utils.GenerateSafeRandomBase64String();
        await _cache.SaveBytesAsync(state, stateBytes, TimeSpan.FromMinutes(5));
        return state;
    }
}