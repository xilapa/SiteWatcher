using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using SiteWatcher.Domain.Users.DTOs;
using SiteWatcher.Infra.Authorization.Constants;

namespace SiteWatcher.IntegrationTests.Utils;

public static class TokenUtils
{
    public static string GenerateFakeGoogleAuthToken(UserViewModel userViewModel)
    {
        var keyByytes = new byte[128];
        foreach (var i in Enumerable.Range(0, 128))
            keyByytes[i] = (byte)i;

        var key = new SymmetricSecurityKey(keyByytes);

        var claims = new Claim[]
        {
            new(AuthenticationDefaults.Google.Id, userViewModel.GetGoogleId()),
            new(AuthenticationDefaults.ClaimTypes.Name, userViewModel.Name),
            new(AuthenticationDefaults.ClaimTypes.Email, userViewModel.Email),
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Issuer = "issuer",
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddSeconds(123),
            SigningCredentials =
                new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature)
        };
        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(token);

        return tokenString;
    }
}