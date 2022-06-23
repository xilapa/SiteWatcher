using System.IdentityModel.Tokens.Jwt;
using SiteWatcher.Application.Authentication.Commands.GoogleAuthentication;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Infra.Authorization.Constants;
using static SiteWatcher.Infra.Http.HttpClientExtensions;

namespace SiteWatcher.Infra.Authorization.GoogleAuth;

public class GoogleAuthService : IGoogleAuthService
{
    private readonly HttpClient _httpClient;
    private readonly IGoogleSettings _googleSettings;

    public GoogleAuthService(IHttpClientFactory httpClientFactory, IGoogleSettings googleSettings)
    {
        _httpClient = httpClientFactory.CreateClient(AuthenticationDefaults.GoogleAuthClient);
        _googleSettings = googleSettings;
    }

    public async Task<GoogleTokenResult> ExchangeCode(string code, CancellationToken cancellationToken)
    {
        var requestBody = new
        {
            code,
            client_id = _googleSettings.ClientId,
            client_secret = _googleSettings.ClientSecret,
            redirect_uri = _googleSettings.RedirectUri,
            grant_type = "authorization_code"
        };

        var response = await _httpClient
            .PostAsync<GoogleTokenMetadata>(_googleSettings.TokenEndpoint, requestBody, cancellationToken);
        if (response is null)
            return new GoogleTokenResult(false);
        var token = new JwtSecurityTokenHandler().ReadJwtToken(response.IdToken);
        var googleId = token.Claims.First(c => c.Type == AuthenticationDefaults.Google.Id).Value;
        var profilePic = token.Claims.FirstOrDefault(c => c.Type == AuthenticationDefaults.Google.Picture)?.Value;
        return new GoogleTokenResult(googleId, profilePic, token.Claims);
    }
}