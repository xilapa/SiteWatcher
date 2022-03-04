using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using SiteWatcher.Domain.Enums;
using SiteWatcher.Domain.Interfaces;
using SiteWatcher.Domain.Models;
using SiteWatcher.WebAPI.Constants;
using SiteWatcher.WebAPI.Extensions;
using SiteWatcher.WebAPI.Settings;

namespace SiteWatcher.WebAPI.Services;

public class TokenService : ITokenService
{
    private readonly AppSettings appSettings;

    public TokenService(AppSettings appSettings)
    {
        this.appSettings = appSettings;
    }

    public string GenerateUserToken(ETokenPurpose purpose, User user)
    {
        var key = purpose switch
        {
            ETokenPurpose.Login => Encoding.ASCII.GetBytes(appSettings.AuthKey),
            _ => throw new ArgumentException("Value out of range", nameof(purpose)),
        };

        var claims = new Claim[] 
        {
            new (AuthenticationDefaults.ClaimTypes.Name, user.Name),
            new (AuthenticationDefaults.ClaimTypes.Email, user.Email),
            new (AuthenticationDefaults.ClaimTypes.EmailConfirmed, user.EmailConfirmed.ToString().ToLower()),
            new (AuthenticationDefaults.ClaimTypes.Language, ((int)user.Language).ToString())            
        };

        return GenerateToken(claims, key);
    }

    public string GenerateRegisterToken(IEnumerable<Claim> tokenClaims, params Claim[] extraClaims)
    {
        var key = Encoding.ASCII.GetBytes(appSettings.RegisterKey);

        var defaultClaims = new Claim[] 
        {
            tokenClaims.GetClaimValue("email"),
            tokenClaims.GetClaimValue("name")
        };

        var claims = defaultClaims.Concat(extraClaims);

        return GenerateToken(claims, key);
    }

    private static string GenerateToken(IEnumerable<Claim> claims, byte[] key)
    {
        var tokenDescriptor = new SecurityTokenDescriptor {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(8),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(token);

        return tokenString;
    }
}