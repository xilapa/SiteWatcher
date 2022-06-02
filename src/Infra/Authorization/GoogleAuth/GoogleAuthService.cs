using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using SiteWatcher.Application.Authentication.Commands.GoogleAuthentication;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Infra.Authorization.Constants;

namespace SiteWatcher.Infra.Authorization.GoogleAuth;

public class GoogleAuthService : IGoogleAuthService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IGoogleSettings _googleSettings;
    private readonly ILogger<GoogleAuthService> _logger;

    public GoogleAuthService(IHttpClientFactory httpClientFactory, IGoogleSettings googleSettings,
        ILogger<GoogleAuthService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _googleSettings = googleSettings;
        _logger = logger;
    }

    public async Task<GoogleTokenResult> ExchangeCode(string code, CancellationToken cancellationToken)
    {
        var httpClient = _httpClientFactory.CreateClient();

        var requestBody = new
        {
            code,
            client_id = _googleSettings.ClientId,
            client_secret = _googleSettings.ClientSecret,
            redirect_uri = _googleSettings.RedirectUri,
            grant_type = "authorization_code"
        };

        // TODO: Substituir chamada da api por Refit e adicionar Polly para retentativas
        using var response = await httpClient.PostAsJsonAsync(_googleSettings.TokenEndpoint, requestBody, cancellationToken);
        if (!response.IsSuccessStatusCode){
            var error = await response.Content.ReadAsStringAsync();
            _logger.LogError("Error on exchanging code at {Date}.\nErrorResponse: {Error}", DateTime.Now, error);
            return new GoogleTokenResult(success: false);
        }

        var tokenResult = await response.Content.ReadFromJsonAsync<GoogleTokenMetadata>();
        var token = new JwtSecurityTokenHandler().ReadJwtToken(tokenResult.IdToken);
        var googleId = token.Claims.First(c => c.Type == AuthenticationDefaults.Google.Id).Value;
        var profilePic = token.Claims.FirstOrDefault(c => c.Type == AuthenticationDefaults.Google.Picture)?.Value;
        return new GoogleTokenResult(googleId, profilePic, token.Claims);
    }
}