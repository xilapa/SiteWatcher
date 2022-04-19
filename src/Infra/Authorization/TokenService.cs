using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Application.Interfaces;
using Microsoft.IdentityModel.Tokens;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Domain.Enums;
using SiteWatcher.Domain.Extensions;
using SiteWatcher.Domain.Models;
using SiteWatcher.Domain.ViewModels;
using SiteWatcher.Infra.Authorization.Constants;

namespace SiteWatcher.Infra.Authorization;

public class TokenService : ITokenService
{
    private readonly IAppSettings _appSettings;
    private readonly ICache _cache;

    private const int RegisterTokenExpiration = 15 * 60;
    private const int LoginTokenExpiration = 8 * 60 * 60;

    public TokenService(IAppSettings appSettings, ICache cache)
    {
        this._appSettings = appSettings;
        this._cache = cache;
    }

    public string GenerateLoginToken(UserViewModel userVm)
    {
        var claims = new Claim[]
        {
            new (AuthenticationDefaults.ClaimTypes.Id, userVm.Id.ToString()),
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

        var tokenDescriptor = new SecurityTokenDescriptor {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddSeconds(expiration),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(token);

        return tokenString;
    }

    public async Task InvalidateToken(string token, ETokenPurpose tokenPurpose)
    {
        var expiration = tokenPurpose switch
        {
            ETokenPurpose.Register => RegisterTokenExpiration,
            ETokenPurpose.Login => LoginTokenExpiration,
            _ => throw new ArgumentException("Value out of range", nameof(tokenPurpose)),
        };
        await _cache.SaveBytesAsync(token.ToBase64String(), _appSettings.InvalidToken, TimeSpan.FromSeconds(expiration));
    }

    public async Task<bool> IsValid(string token)
    {
        var value = await _cache.GetBytesAsync(token.ToBase64String());
        return !_appSettings.InvalidToken.SequenceEqual(value ?? Array.Empty<byte>());
    }
}