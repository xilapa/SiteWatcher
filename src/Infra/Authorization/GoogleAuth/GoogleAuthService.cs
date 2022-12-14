using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SiteWatcher.Application.Authentication.Commands.GoogleAuthentication;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Infra.Authorization.Constants;
using SiteWatcher.Infra.Http;
using static SiteWatcher.Infra.Http.HttpRetryPolicies;

namespace SiteWatcher.Infra.Authorization.GoogleAuth;

public class GoogleAuthService : IGoogleAuthService
{
    private readonly System.Net.Http.HttpClient _httpClient;
    private readonly IGoogleSettings _googleSettings;
    private readonly ILogger<GoogleAuthService> _logger;

    public GoogleAuthService(IHttpClientFactory httpClientFactory, IGoogleSettings googleSettings,
        ILogger<GoogleAuthService> logger)
    {
        _httpClient = httpClientFactory.CreateClient(AuthenticationDefaults.GoogleAuthClient);
        _googleSettings = googleSettings;
        _logger = logger;
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
            .PostAsync<GoogleTokenMetadata>(_googleSettings.TokenEndpoint, requestBody, _logger,
                TransientErrorsRetryWithTimeout, cancellationToken);
        if (response is null)
            return new GoogleTokenResult(false);
        var token = new JwtSecurityTokenHandler().ReadJwtToken(response.IdToken);
        var googleId = token.Claims.First(c => c.Type == AuthenticationDefaults.Google.Id).Value;
        var profilePic = token.Claims.FirstOrDefault(c => c.Type == AuthenticationDefaults.Google.Picture)?.Value;
        return new GoogleTokenResult(googleId, profilePic, token.Claims);
    }
}