using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using SiteWatcher.Domain.Enums;
using SiteWatcher.Domain.Extensions;
using SiteWatcher.Domain.Interfaces;
using SiteWatcher.Domain.Models;
using SiteWatcher.Domain.ViewModels;
using SiteWatcher.WebAPI.Constants;
using SiteWatcher.WebAPI.Extensions;
using SiteWatcher.WebAPI.Settings;

namespace SiteWatcher.WebAPI.Services;

public class TokenService : ITokenService
{
    private readonly AppSettings appSettings;
    private readonly ICache cache;

    private readonly int registerTokenExpiration = 15 * 60;
    private readonly int loginTokenExpiration = 8 * 60 * 60;

    public TokenService(AppSettings appSettings, ICache cache) 
    {
        this.appSettings = appSettings;
        this.cache = cache;
    }  

    public string GenerateLoginToken(UserViewModel userVM)
    {
        var claims = new Claim[]
        {
            new (AuthenticationDefaults.ClaimTypes.Id, userVM.Id.ToString()),
            new (AuthenticationDefaults.ClaimTypes.Name, userVM.Name),
            new (AuthenticationDefaults.ClaimTypes.Email, userVM.Email),
            new (AuthenticationDefaults.ClaimTypes.EmailConfirmed, userVM.EmailConfirmed.ToString().ToLower()),
            new (AuthenticationDefaults.ClaimTypes.Language, ((int)userVM.Language).ToString())            
        };

        return GenerateToken(claims, ETokenPurpose.Login, loginTokenExpiration);
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

        return GenerateToken(claims, ETokenPurpose.Login, loginTokenExpiration);
    }

    public string GenerateRegisterToken(IEnumerable<Claim> tokenClaims, string googleId)
    {   
        var locale = tokenClaims.DefaultIfEmpty(new Claim(AuthenticationDefaults.ClaimTypes.Locale, string.Empty))
                                     .FirstOrDefault(c => c.Type == AuthenticationDefaults.ClaimTypes.Locale).Value
                                     .Split("-").First();    

        var claims = new Claim[] 
        {
            tokenClaims.GetClaimValue(AuthenticationDefaults.ClaimTypes.Name),
            tokenClaims.GetClaimValue(AuthenticationDefaults.ClaimTypes.Email),
            new Claim(AuthenticationDefaults.ClaimTypes.Language, ((int)locale.GetEnumValue<ELanguage>()).ToString()),
            new Claim(AuthenticationDefaults.ClaimTypes.GoogleId, googleId)
        };

        return GenerateToken(claims, ETokenPurpose.Register, registerTokenExpiration);   
    }

    private string GenerateToken(IEnumerable<Claim> claims, ETokenPurpose tokenPurpose, int expiration)
    {
        var key = tokenPurpose switch
        {
            ETokenPurpose.Register => appSettings.RegisterKey,
            ETokenPurpose.Login => appSettings.AuthKey,
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
            ETokenPurpose.Register => registerTokenExpiration,
            ETokenPurpose.Login => loginTokenExpiration,
            _ => throw new ArgumentException("Value out of range", nameof(tokenPurpose)),
        };
        await cache.SaveBytesAsync(token.ToBase64tring(), appSettings.InvalidToken, TimeSpan.FromSeconds(expiration));
    }

    public async Task<bool> IsValid(string token)
    {
        var value = await cache.GetBytesAsync(token.ToBase64tring());
        if(value is null)
            return true;

        if(value.SequenceEqual(appSettings.InvalidToken))
            return false;

        return true;
    }   
    
}