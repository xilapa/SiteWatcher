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

    public TokenService(AppSettings appSettings) =>  this.appSettings = appSettings;

    public string GenerateUserToken(ETokenPurpose tokenPurpose, User user)
    {
        var claims = new Claim[] 
        {
            new (AuthenticationDefaults.ClaimTypes.Name, user.Name),
            new (AuthenticationDefaults.ClaimTypes.Email, user.Email),
            new (AuthenticationDefaults.ClaimTypes.EmailConfirmed, user.EmailConfirmed.ToString().ToLower()),
            new (AuthenticationDefaults.ClaimTypes.Language, ((int)user.Language).ToString())            
        };

        return GenerateToken(claims, tokenPurpose);
    }

    public string GenerateRegisterToken(IEnumerable<Claim> tokenClaims, params Claim[] extraClaims)
    {        
        var defaultClaims = new Claim[] 
        {
            tokenClaims.GetClaimValue(AuthenticationDefaults.ClaimTypes.Name),
            tokenClaims.GetClaimValue(AuthenticationDefaults.ClaimTypes.Email)
        };

        var claims = defaultClaims.Concat(extraClaims);

        return GenerateToken(claims, ETokenPurpose.Register);   
    }

    private string GenerateToken(IEnumerable<Claim> claims, ETokenPurpose tokenPurpose)
    {
        var key = tokenPurpose switch
        {
            ETokenPurpose.Register => appSettings.RegisterKey,
            ETokenPurpose.Login => appSettings.AuthKey,
            _ => throw new ArgumentException("Value out of range", nameof(tokenPurpose)),
        };

        var tokenDescriptor = new SecurityTokenDescriptor {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(8),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.ASCII.GetBytes(key)), SecurityAlgorithms.HmacSha256Signature)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(token);

        return tokenString;
    }
}